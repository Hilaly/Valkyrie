using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Configs;
using Meta.Commands;
using Meta.Inventory;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.MVVM.Bindings;
using Valkyrie.Tools;
using Valkyrie.Utils.Pool;

namespace Valkyrie
{
    public interface INamed
    {
        public string Name { get; set; }
    }
    
    public class MemberInfo : INamed
    {
        public string Name { get; set; }
        public string Type;
    }

    public class PropertyInfo : MemberInfo
    {
        public bool IsRequired;
    }

    public class InfoGetter : MemberInfo
    {
        public string Code;
    }

    public class EventEntity : INamed
    {
        public string Name { get; set; }
        public readonly List<string> Args = new();

        public string ClassName => $"{Name}Event";

        public void Write(FormatWriter sb)
        {
            var blockName = $"public sealed class {ClassName} : {typeof(BaseEvent).FullName}";
            if (Args.Any())
                blockName += $"<{Args.Join(", ")}>";
            sb.BeginBlock(blockName);
            sb.EndBlock();
        }
    }

    public interface IType : INamed
    {
        public HashSet<string> Attributes { get; }
        public IType AddProperty(string type, string name, bool isRequired);
    }
    
    public abstract class Named : INamed, IType
    {
        public HashSet<string> Attributes { get; } = new();
        public string Name { get; set; }
        public abstract IType AddProperty(string type, string name, bool isRequired);

        protected internal readonly List<PropertyInfo> Properties = new();

        public virtual IReadOnlyList<PropertyInfo> GetAllProperties()
        {
            return Properties;
        }
    }

    public abstract class InheritedNamed<T> : Named where T : Named
    {
        protected readonly List<T> BaseTypes = new();

        public override IReadOnlyList<PropertyInfo> GetAllProperties()
        {
            var r = new List<PropertyInfo>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllProperties()))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Properties)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            return r;
        }
        
        public InheritedNamed<T> Inherit(T baseType)
        {
            BaseTypes.Add(baseType);
            return this;
        }

    }

    public class ItemEntity : InheritedNamed<ItemEntity>, IType
    {
        private string BaseInterfaceName = typeof(BaseInventoryItem).FullName;

        public ItemEntity AddProperty(string type, string name)
        {
            Properties.Add(new PropertyInfo()
            {
                Name = name,
                Type = type
            });

            return this;
        }

        public override IType AddProperty(string type, string name, bool isRequired) => AddProperty(type, name);

        public void Write(FormatWriter sb)
        {
            var propertyAttributes = this.Attributes.Contains("view")
                ? $"[{typeof(BindingAttribute).FullName}] "
                : string.Empty;
            var blockName = $"{propertyAttributes}public partial class {Name} : ";
            if (BaseTypes.Count > 0)
                blockName += BaseTypes.Select(x => x.Name).Join(", ");
            else
                blockName += BaseInterfaceName;
            sb.BeginBlock(blockName);
            
            foreach (var property in Properties)
                sb.AppendLine($"{propertyAttributes}public {property.Type} {property.Name} {{ get; set; }}");

            sb.EndBlock();
        }
    }

    public class ConfigEntity : InheritedNamed<ConfigEntity>, IType
    {
        private string BaseInterfaceName => typeof(IConfigData).FullName;

        public void Write(FormatWriter sb)
        {
            var blockName = $"public class {Name} : ";
            if (BaseTypes.Count > 0)
                blockName += string.Join(", ", BaseTypes.Select(x => x.Name)) + ", ";
            blockName += BaseInterfaceName;
            sb.BeginBlock(blockName);

            sb.AppendLine($"#region {BaseInterfaceName}");
            sb.AppendLine();
            if (BaseTypes.Any())
            {
                sb.BeginBlock($"public override void PastLoad(IDictionary<string, {BaseInterfaceName}> configData)");
                sb.AppendLine("base.PastLoad(configData);");
                sb.EndBlock();
            }
            else
            {
                sb.AppendLine("public string Id;");
                sb.AppendLine($"public string GetId() => Id;");
                sb.BeginBlock($"public virtual void PastLoad(IDictionary<string, {BaseInterfaceName}> configData)");
                sb.EndBlock();
            }

            sb.AppendLine();
            sb.AppendLine($"#endregion //{BaseInterfaceName}");
            sb.AppendLine();

            foreach (var property in Properties)
                sb.AppendLine($"public {property.Type} {property.Name};");

            sb.EndBlock();
        }

        public ConfigEntity AddProperty(string type, string name)
        {
            Properties.Add(new PropertyInfo()
            {
                Name = name,
                Type = type
            });

            return this;
        }

        public ConfigEntity Inherit(ConfigEntity baseType)
        {
            BaseTypes.Add(baseType);
            return this;
        }

        public override IType AddProperty(string type, string name, bool isRequired) => AddProperty(type, name);
    }

    public abstract class EntityBase : IType
    {
        public HashSet<string> Attributes { get; } = new();
        public string Name { get; set; }
        protected readonly List<EntityBase> BaseTypes = new();
        protected readonly List<PropertyInfo> Properties = new();
        internal readonly List<string> Timers = new();
        protected readonly List<InfoGetter> Infos = new();
        protected readonly List<MemberInfo> Configs = new();
        protected readonly List<MemberInfo> Slots = new();

        protected readonly List<string> SyncWithPrefabs = new();

        public IReadOnlyList<string> GetPrefabsProperties() => SyncWithPrefabs;

        public abstract void Write(FormatWriter sb);

        public IReadOnlyList<PropertyInfo> GetAllProperties()
        {
            var r = new List<PropertyInfo>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllProperties()))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Properties)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            return r;
        }

        public IReadOnlyList<MemberInfo> GetAllConfigs()
        {
            var r = new List<MemberInfo>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllConfigs()))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Configs)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            return r;
        }

        public IReadOnlyList<MemberInfo> GetAllSlots()
        {
            var r = new List<MemberInfo>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllSlots()))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Slots)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            return r;
        }

        public IReadOnlyList<InfoGetter> GetAllInfos()
        {
            var r = new List<InfoGetter>();

            foreach (var propertyInfo in BaseTypes.SelectMany(entityBase => entityBase.GetAllInfos()))
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            foreach (var propertyInfo in Infos)
                if (!r.Contains(propertyInfo))
                    r.Add(propertyInfo);

            return r;
        }

        public IReadOnlyCollection<string> GetAllTimers()
        {
            var s = new HashSet<string>(Timers);
            foreach (var baseType in BaseTypes)
                s.UnionWith(baseType.GetAllTimers());
            return s;
        }

        public EntityBase Inherit(EntityBase parent)
        {
            if (!BaseTypes.Contains(parent))
                BaseTypes.Add(parent);

            return this;
        }

        IType IType.AddProperty(string type, string name, bool isRequired) => AddProperty(type, name, isRequired);

        public EntityBase AddProperty(string type, string name, bool isRequired = true)
        {
            Properties.Add(new PropertyInfo()
            {
                Name = name,
                Type = type,
                IsRequired = isRequired
            });

            return this;
        }

        public EntityBase AddTimer(string name)
        {
            if (!Timers.Contains(name))
                Timers.Add(name);
            return this;
        }

        public EntityBase AddInfo(string type, string name, string code)
        {
            Infos.Add(new InfoGetter()
            {
                Name = name,
                Type = type,
                Code = code
            });
            return this;
        }

        public EntityBase AddConfig(string type, string name)
        {
            Configs.Add(new MemberInfo()
            {
                Name = name,
                Type = type
            });
            return this;
        }

        public EntityBase AddSlot(string type, string name)
        {
            Slots.Add(new MemberInfo()
            {
                Name = name,
                Type = type
            });
            return this;
        }

        public EntityBase AddSlot(EntityBase type, string name) => AddSlot(type.Name, name);

        public virtual EntityBase Singleton() => this;

        public EntityBase ViewWithPrefabByProperty(string propertyName)
        {
            SyncWithPrefabs.Add(propertyName);
            return View();
        }

        protected void WriteViewModels(FormatWriter sb)
        {
            if (HasView)
            {
                sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public partial class {Name}ViewModel");
                sb.AppendLine($"public {Name} Model {{ get; }}");
                sb.BeginBlock($"public {Name}ViewModel({Name} model)");
                sb.AppendLine("Model = model;");
                sb.EndBlock();
                foreach (var property in GetAllProperties())
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public {property.Type} {property.Name} => Model.{property.Name};");
                foreach (var info in GetAllInfos())
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public {info.Type} {info.Name} => Model.{info.Name};");
                foreach (var info in GetAllSlots())
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public {info.Type} {info.Name} => Model.{info.Name};");
                foreach (var timer in GetAllTimers())
                {
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public bool HasTimer{timer} => Model.{timer} != null;");
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public float {timer}TimeLeft => Model.{timer}?.TimeLeft ?? 0f;");
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public float {timer}Time => {timer}FullTime - {timer}TimeLeft;");
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public float {timer}FullTime => Model.{timer}?.FullTime ?? 1f;");
                    sb.AppendLine(
                        $"[{typeof(BindingAttribute).FullName}] public float {timer}Progress => Mathf.Clamp01({timer}Time / {timer}FullTime);");
                }

                foreach (var info in GetAllConfigs())
                    sb.AppendLine($"public {info.Type} {info.Name} => Model.{info.Name};");
                sb.EndBlock();
            }
        }

        public bool HasView { get; private set; }

        public EntityBase View()
        {
            HasView = true;
            return this;
        }

        public IEnumerable<EntityBase> GetAllImplemented() =>
            new HashSet<EntityBase>(BaseTypes.SelectMany(entityBase => entityBase.GetAllImplemented())) { this };
    }

    public class EntityInterface : EntityBase
    {
        public override void Write(FormatWriter sb)
        {
            var blockName = $"public interface {Name} : IEntity";
            if (BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in Properties)
                sb.AppendLine($"public {property.Type} {property.Name} {{ get; set; }}");
            foreach (var timer in Timers)
            {
                sb.AppendLine($"ITimer {timer} {{ get; }}");
                sb.AppendLine($"void Start{timer}(float time);");
                sb.AppendLine($"void Stop{timer}();");
                sb.AppendLine($"bool {timer}JustFinished {{ get; }}");
            }

            foreach (var info in Configs)
                sb.AppendLine($"public {info.Type} {info.Name} {{ get; set; }}");

            foreach (var info in Slots)
                sb.AppendLine($"public {info.Type} {info.Name} {{ get; set; }}");

            foreach (var info in Infos)
                sb.AppendLine($"public {info.Type} {info.Name} {{ get; }}");

            sb.EndBlock();

            WriteViewModels(sb);
        }
    }

    public class EntityInfo : EntityBase
    {
        public bool IsSingleton;

        public override EntityBase Singleton()
        {
            IsSingleton = true;
            return base.Singleton();
        }

        public override void Write(FormatWriter sb)
        {
            var blockName = $"public partial class {Name} : IEntity";
            if (BaseTypes.Count > 0)
                blockName += ", " + string.Join(", ", BaseTypes.Select(x => x.Name));
            sb.BeginBlock(blockName);

            foreach (var property in GetAllProperties())
                sb.AppendLine($"public {property.Type} {property.Name} {{ get; set; }}");
            foreach (var property in GetAllConfigs())
                sb.AppendLine($"public {property.Type} {property.Name} {{ get; set; }}");
            foreach (var property in GetAllSlots())
                sb.AppendLine($"public {property.Type} {property.Name} {{ get; set; }}");
            foreach (var property in GetAllInfos())
                sb.AppendLine($"public {property.Type} {property.Name} => {property.Code};");

            var timers = GetAllTimers();
            foreach (var timer in timers)
            {
                sb.AppendLine($"private EntityTimer {timer.ConvertToCamelCaseFieldName()};");
                sb.AppendLine(
                    $"public ITimer {timer} => {timer.ConvertToCamelCaseFieldName()} is {{ TimeLeft: > 0 }} ? {timer.ConvertToCamelCaseFieldName()} : {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.BeginBlock($"public void Start{timer}(float time)");
                sb.AppendLine($"if ({timer} != null) throw new Exception(\"Timer {timer} already exist\");");
                sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = new EntityTimer(time);");
                sb.EndBlock();
                sb.AppendLine($"public void Stop{timer}() => {timer.ConvertToCamelCaseFieldName()} = default;");
                sb.AppendLine($"public bool {timer}JustFinished {{ get; private set; }}");
            }

            if (timers.Any())
            {
                sb.BeginBlock("internal void AdvanceTimers(float dt)");
                foreach (var timer in timers)
                {
                    sb.AppendLine($"{timer}JustFinished = false;");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()} != null)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()}.Advance(dt);");
                    sb.BeginBlock($"if({timer.ConvertToCamelCaseFieldName()}.TimeLeft <= 0)");
                    sb.AppendLine($"{timer.ConvertToCamelCaseFieldName()} = default;");
                    sb.AppendLine($"{timer}JustFinished = true;");
                    sb.EndBlock();
                    sb.EndBlock();
                }

                sb.EndBlock();
            }

            sb.EndBlock();

            WriteViewModels(sb);
        }
    }

    public abstract class EventHandlerOperation
    {
        public abstract bool IsAsync();
        public abstract void Write(FormatWriter sb, OpType opType);
    }

    class LogOperation : EventHandlerOperation
    {
        private readonly string _text;

        public LogOperation(string text)
        {
            _text = text;
        }

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine($"UnityEngine.Debug.Log(\"[GEN]: {_text}\");");
        }

        public override bool IsAsync() => false;
    }

    class RaiseEventOperation : EventHandlerOperation
    {
        private readonly EventEntity _raisedEvent;
        private readonly string[] _args;

        public RaiseEventOperation(EventEntity raisedEvent, string[] args)
        {
            _raisedEvent = raisedEvent;
            _args = args;

            if (_raisedEvent.Args.Count != args.Length)
                throw new Exception($"To raise {raisedEvent.Name} need to use {raisedEvent.Args.Count} args");
        }

        public override bool IsAsync() => true;

        public override void Write(FormatWriter sb, OpType opType)
        {
            var args = string.Empty;
            for (var i = 0; i < _args.Length; ++i)
                args += $", Arg{i} = {_args[i]}";
            if (args.Length > 0)
                args = args[2..];
            sb.AppendLine($"await Raise(new {_raisedEvent.ClassName} {{ {args} }});");
        }
    }

    class WriteCodeLine : EventHandlerOperation
    {
        private string _code;

        public override bool IsAsync() => false;

        public WriteCodeLine(string code)
        {
            if (code.EndsWith(";"))
                _code = code;
            else
                _code = code + ";";
        }

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine(_code);
        }
    }

    class CallCommandOperation : EventHandlerOperation
    {
        private readonly string _command;
        private readonly List<string> _args;

        public CallCommandOperation(string command, string[] args)
        {
            _command = command;
            _args = new List<string>(args);
        }

        public override bool IsAsync() => true;

        public override void Write(FormatWriter sb, OpType opType)
        {
            sb.AppendLine($"await Interpreter.Execute({_command}{_args.Select(x => $", {x}").Join(string.Empty)});");
        }
    }

    public class MethodImpl
    {
        protected readonly List<EventHandlerOperation> _ops = new();

        public MethodImpl LogOp(string text)
        {
            _ops.Add(new LogOperation(text));
            return this;
        }

        public MethodImpl RaiseOp(EventEntity raisedEvent, params string[] args)
        {
            _ops.Add(new RaiseEventOperation(raisedEvent, args));
            return this;
        }

        public MethodImpl CommandOp(string command, params string[] args)
        {
            _ops.Add(new CallCommandOperation(command, args));
            return this;
        }

        public MethodImpl CodeOp(string code)
        {
            _ops.Add(new WriteCodeLine(code));
            return this;
        }
    }

    public class WindowHandler : MethodImpl
    {
        public string Name { get; set; }

        public void Write(FormatWriter sb)
        {
            foreach (var op in _ops)
                op.Write(sb, OpType.Window);
        }
    }

    public enum OpType
    {
        Window,
        Handler
    }

    public class EventHandlerModel : MethodImpl
    {
        public readonly EventEntity Event;
        private readonly string _uid;

        public EventHandlerModel(EventEntity @event)
        {
            _uid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            Event = @event;
        }

        public void Write(FormatWriter sb)
        {
            bool isAsync = _ops.Any(x => x.IsAsync());
            sb.BeginBlock(
                $"{(isAsync ? "async " : string.Empty)}System.Threading.Tasks.Task {GetMethodName()}({Event.ClassName} ev)");
            foreach (var op in _ops)
                op.Write(sb, OpType.Handler);
            if (!isAsync)
                sb.AppendLine("return System.Threading.Tasks.Task.CompletedTask;");
            sb.EndBlock();
        }

        public string GetMethodName()
        {
            return $"On{Event.ClassName}Handle{_uid}";
        }
    }

    public class ItemFilterModel
    {
        public string Name;
        public ItemEntity Entity;
        public ItemFilterModel Source;
        public string Code;
    }

    public class ProfileModel
    {
        public HashSet<string> Counters = new();
        public List<EventHandlerModel> Handlers = new();
        public List<ItemEntity> Items = new();
        public List<ItemFilterModel> Filters = new();

        public ProfileModel AddCounter(string name)
        {
            Counters.Add(name);
            return this;
        }

        public void WriteEvents(FormatWriter sb)
        {
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Inherit from this class and use Where<T>(Handler)");
            sb.AppendLine($"/// </summary>");
            sb.BeginBlock("public abstract class BaseEventsHandler : IDisposable");
            sb.AppendLine($"private readonly {typeof(CompositeDisposable).FullName} _disposable = new();");
            sb.AppendLine($"private readonly {typeof(IEventSystem).FullName} _events;");
            sb.AppendLine($"protected IPlayerProfile Profile {{ get; }}");
            sb.AppendLine($"protected {typeof(ICommandsInterpreter).FullName} Interpreter {{ get; }}");
            sb.AppendLine($"protected {typeof(IConfigService).FullName} Config {{ get; }}");
            sb.BeginBlock(
                $"protected BaseEventsHandler({typeof(IEventSystem).FullName} events, IPlayerProfile profile, {typeof(ICommandsInterpreter).FullName} interpreter, {typeof(IConfigService).FullName} config)");
            sb.AppendLine($"_events = events;");
            sb.AppendLine($"Profile = profile;");
            sb.AppendLine($"Interpreter = interpreter;");
            sb.AppendLine($"Config = config;");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("public virtual void Dispose() => _disposable.Dispose();");
            sb.AppendLine();
            sb.BeginBlock(
                $"protected {typeof(Task).FullName} Raise<T>(T instance) where T : {typeof(BaseEvent).FullName}");
            sb.AppendLine("Debug.Log($\"[GEN]: Raise {typeof(T).Name} event from {GetType().Name}\");");
            sb.AppendLine("return _events.Raise(instance);");
            sb.EndBlock();
            sb.AppendLine($"protected void When<T>(Func<T, {typeof(Task).FullName}> handler) where T : {typeof(BaseEvent).FullName} => _disposable.Add(_events.Subscribe<T>(handler));");
            sb.AppendLine();
            sb.AppendLine($"protected void When<T>(Action<T> handler) where T : {typeof(BaseEvent).FullName} => _disposable.Add(_events.Subscribe<T>(handler));");
            sb.EndBlock();
            sb.AppendLine();

            sb.BeginBlock("class GeneratedEventsHandler : BaseEventsHandler");
            sb.AppendLine(
                $"public GeneratedEventsHandler(IPlayerProfile profile, {typeof(IEventSystem).FullName} eventSystem, {typeof(ICommandsInterpreter).FullName} interpreter, {typeof(IConfigService).FullName} config)");
            sb.BeginBlock(": base(eventSystem, profile, interpreter, config)");
            foreach (var handler in Handlers)
                sb.AppendLine($"When<{handler.Event.ClassName}>({handler.GetMethodName()});");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("#region Handlers");
            sb.AppendLine();
            foreach (var handler in Handlers)
                handler.Write(sb);
            sb.AppendLine();
            sb.AppendLine("#endregion //Handlers");
            sb.EndBlock();
        }

        public void Write(FormatWriter sb)
        {
            sb.AppendLine("#region Items");
            sb.AppendLine();
            foreach (var item in Items) 
                item.Write(sb);
            sb.AppendLine();
            sb.AppendLine("#endregion //Items");
            sb.AppendLine();
            
            sb.BeginBlock("public static class GeneratedConstants");
            foreach (var counter in Counters)
            {
                sb.AppendLine($"public const string {counter}Name = \"{counter}\";");
            }
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock("public interface IPlayerProfile");
            sb.AppendLine("#region Counters");
            sb.AppendLine();
            sb.AppendLine($"public {typeof(BigInteger).FullName} this[string counterId] {{ get; set; }}");
            foreach (var counter in Counters)
                sb.AppendLine($"public {typeof(BigInteger).FullName} {counter} {{ get; set; }}");
            sb.AppendLine();
            sb.AppendLine("#endregion //Counters");
            sb.AppendLine();
            sb.AppendLine($"T Add<T>(T item) where T : {typeof(IInventoryItem).FullName};");
            sb.AppendLine();
            sb.AppendLine("#region Filters");
            sb.AppendLine();
            sb.AppendLine($"IEnumerable<{typeof(IInventoryItem).FullName}> All {{ get; }}");
            sb.AppendLine();
            foreach (var item in Items) 
                sb.AppendLine($"{item.Name} {item.Name}(string id);");
            sb.AppendLine();
            foreach (var filter in Filters) 
                sb.AppendLine($"IEnumerable<{filter.Entity.Name}> {filter.Name} {{ get; }}");
            sb.AppendLine();
            sb.AppendLine("#endregion //Filters");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock("class PlayerProfile : IPlayerProfile");
            sb.AppendLine($"readonly {typeof(IWallet).FullName} _wallet;");
            sb.AppendLine($"readonly {typeof(IInventory).FullName} _inventory;");
            //TODO: other references
            sb.AppendLine();
            sb.BeginBlock(
                $"public PlayerProfile({typeof(IWallet).FullName} wallet, {typeof(IInventory).FullName} inventory)");
            sb.AppendLine("_wallet = wallet;");
            sb.AppendLine("_inventory = inventory;");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("#region Counters");
            sb.AppendLine();
            sb.BeginBlock($"public {typeof(BigInteger).FullName} this[string counterId]");
            sb.AppendLine($"get => _wallet.GetBigAmount(counterId);");
            sb.AppendLine($"set => _wallet.SetAmount(counterId, value);");
            sb.EndBlock();
            foreach (var counter in Counters)
            {
                sb.BeginBlock($"public {typeof(BigInteger).FullName} {counter}");
                sb.AppendLine($"get => _wallet.GetBigAmount(GeneratedConstants.{counter}Name);");
                sb.AppendLine($"set => _wallet.SetAmount(GeneratedConstants.{counter}Name, value);");
                sb.EndBlock();
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //Counters");
            sb.AppendLine();
            sb.BeginBlock($"public T Add<T>(T item) where T : {typeof(IInventoryItem).FullName}");
            sb.AppendLine("_inventory.Add(item);");
            sb.AppendLine("return item;");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("#region Filters");
            sb.AppendLine();
            sb.AppendLine($"public IEnumerable<{typeof(IInventoryItem).FullName}> All => _inventory.Get();");
            sb.AppendLine();
            foreach (var item in Items) 
                sb.AppendLine($"public {item.Name} {item.Name}(string id) => _inventory.Get<{item.Name}>(id);");
            sb.AppendLine();
            foreach (var filter in Filters)
            {
                sb.AppendLine($"public IEnumerable<{filter.Entity.Name}> {filter.Name} =>");
                sb.AddTab();
                var source = filter.Source != null ? filter.Source.Name : $"_inventory.Get<{filter.Entity.Name}>()";
                sb.AppendLine($"{source}");
                if (filter.Code.NotNullOrEmpty())
                {
                    sb.BeginBlock($".Where({filter.Entity.Name.ToLowerInvariant()} =>");
                    sb.AppendLine("var Profile = this;");
                    sb.AppendLine($"return {filter.Code};");
                    sb.EndBlock();
                    sb.AppendLine(");");
                }
                else
                    sb.AppendLine(";");

                sb.RemoveTab();
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //Filters");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"public class MetaLibrary : {typeof(ILibrary).FullName}");
            sb.BeginBlock($"public void Register({typeof(IContainer).FullName} container)");
            sb.AppendLine("container.Register<PlayerProfile>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine(
                "container.Register<GeneratedEventsHandler>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.EndBlock();
            sb.EndBlock();
        }
    }

    public class WorldModelInfo
    {
        public string Namespace = nameof(WorldModelInfo);

        public List<EntityBase> Entities = new();
        public List<ConfigEntity> Configs = new();
        public List<EventEntity> Events = new();
        public List<WindowModelInfo> Windows = new();
        public ProfileModel Profile = new();

        public void WriteToDirectory(string dirPath, string mainCsFile = "Gen.cs")
        {
            //1. Serialize all
            var rootNamespace = Namespace;
            var methods = new List<KeyValuePair<string, string>>
            {
                new(mainCsFile, ToString(false))
            };
            foreach (var window in Windows)
            {
                var sb = new FormatWriter();
                WriteFileStart(sb);
                sb.BeginBlock($"namespace {rootNamespace}");
                window.Write(sb);
                sb.EndBlock();
                methods.Add(new KeyValuePair<string, string>($"{window.ClassName}.cs", sb.ToString()));
            }

            //2. Prepare directory
            Debug.Log($"[GENERATION]: Writing to directory {dirPath}");
            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);
            foreach (var filePath in Directory.EnumerateFiles(dirPath, "*.cs", SearchOption.AllDirectories))
            {
                Debug.Log($"[GENERATION]: remove file {filePath}");
                File.Delete(filePath);
            }

            //3. Write files
            foreach (var (fileName, text) in methods)
            {
                var fullPath = Path.Combine(dirPath, fileName);
                Debug.Log($"[GENERATION]: writing to file {fullPath}");
                File.WriteAllText(fullPath, text);
            }

            Debug.Log($"[GENERATION]: SUCCESS in {dirPath}");
        }

        private string ToString(bool includeMono)
        {
            var sb = new FormatWriter();

            WriteFileStart(sb);

            var rootNamespace = Namespace;

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Ui");
            sb.AppendLine();
            if (includeMono)
                WriteUi(sb);
            WriteBaseClassesToImplement(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Ui");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Events");
            sb.AppendLine();
            WriteEvents(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Events");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Profile");
            sb.AppendLine();
            Profile.Write(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Profile");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region ConfigData");
            sb.AppendLine();
            WriteConfigs(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //ConfigData");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Entities");
            sb.AppendLine();
            WriteEntities(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Entities");
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Simulation");
            sb.AppendLine();
            WriteGeneral(sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Simulation");
            sb.EndBlock();

            return sb.ToString();
        }

        private void WriteBaseClassesToImplement(FormatWriter sb)
        {
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// All windows must inherit from this class");
            sb.AppendLine($"/// </summary>");
            sb.BeginBlock($"public abstract class ProjectWindow : {typeof(BaseWindow).FullName}");
            sb.AppendLine($"[field: {typeof(InjectAttribute).FullName}] public IPlayerProfile Profile {{ get; }}");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine($"/// <summary>");
            sb.AppendLine($"/// Inherit Views from this class");
            sb.AppendLine($"/// </summary>");
            sb.BeginBlock($"public abstract class ProjectView : {typeof(BaseView).FullName}");
            sb.AppendLine($"[field: {typeof(InjectAttribute).FullName}] public IPlayerProfile Profile {{ get; }}");
            sb.EndBlock();
        }

        private static void WriteFileStart(FormatWriter sb)
        {
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using Valkyrie;");
            sb.AppendLine();
        }

        public override string ToString()
        {
            return ToString(true);
        }

        private void WriteUi(FormatWriter sb)
        {
            foreach (var window in Windows)
                window.Write(sb);
        }

        private void WriteEvents(FormatWriter sb)
        {
            foreach (var entity in Events)
                entity.Write(sb);

            sb.AppendLine();

            Profile.WriteEvents(sb);
        }

        void WriteConfigs(FormatWriter sb)
        {
            foreach (var entity in Configs)
                entity.Write(sb);
        }

        private void WriteEntities(FormatWriter sb)
        {
            foreach (var entityInfo in Entities)
                entityInfo.Write(sb);

            var allTimers = GetAllTimers();
            foreach (var timer in allTimers)
            {
                sb.BeginBlock($"public interface I{timer.Type.Name}{timer.Timer}Handler");
                sb.AppendLine(
                    $"void On{timer.Type.Name}{timer.Timer}Finish({timer.Type.Name} {timer.Type.Name.ConvertToUnityPropertyName()});");
                sb.EndBlock();
            }
        }

        private void WriteGeneral(FormatWriter sb)
        {
            var allTimers = GetAllTimers();

            WriteInterfaces(sb, allTimers);

            sb.BeginBlock("class EntityTimer : ITimer");
            sb.AppendLine("public float FullTime { get; }");
            sb.AppendLine("public float TimeLeft { get; private set; }");
            sb.BeginBlock("public EntityTimer(float time)");
            sb.AppendLine("FullTime = TimeLeft = time;");
            sb.EndBlock();
            sb.AppendLine("public void Advance(float dt) => TimeLeft -= dt;");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldState : IWorldState");
            sb.AppendLine("public readonly List<IEntity> Entities = new();");
            sb.AppendLine("public readonly HashSet<IEntity> ToDestroy = new();");
            sb.AppendLine("public IReadOnlyList<IEntity> All => Entities;");
            foreach (var entityInfo in Entities)
            {
                sb.AppendLine($"public readonly List<{entityInfo.Name}> _allOf{entityInfo.Name} = new();");
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} => _allOf{entityInfo.Name}; // Entities.OfType<{entityInfo.Name}>().ToList();");
            }

            foreach (var entityInfo in Entities.Where(x => x is EntityInfo { IsSingleton: true }))
                sb.AppendLine(
                    $"public {entityInfo.Name} {entityInfo.Name} => ({entityInfo.Name})Entities.Find(x => x is {entityInfo.Name});");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldController : IWorldController");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly WorldView _worldView;");
            sb.BeginBlock("public WorldController(WorldState worldState, WorldView worldView)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_worldView = worldView;");
            sb.EndBlock();
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllProperties().Where(x => x.IsRequired).ToList();
                var argsStr = string.Join(", ", args.Select(x => $"{x.Type} {x.Name.ConvertToUnityPropertyName()}"));
                sb.BeginBlock($"public {entityInfo.Name} Create{entityInfo.Name}({argsStr})");
                if (entityInfo.IsSingleton)
                    sb.AppendLine(
                        $"if(_worldState.Entities.Find(x => x is {entityInfo.Name}) != null) throw new Exception(\"{entityInfo.Name} already exists\");");
                sb.BeginBlock($"var result = new {entityInfo.Name}");
                foreach (var propertyInfo in args)
                    sb.AppendLine($"{propertyInfo.Name} = {propertyInfo.Name.ConvertToUnityPropertyName()},");
                sb.EndBlock();
                sb.AppendLine(";");
                sb.AppendLine("_worldState.Entities.Add(result);");
                sb.BeginBlock($"//Update Caches");
                foreach (var entityBase in entityInfo.GetAllImplemented())
                    sb.AppendLine($"_worldState._allOf{entityBase.Name}.Add(result);");
                sb.EndBlock();
                sb.BeginBlock($"//Spawn views");
                foreach (var type in entityInfo.GetAllImplemented())
                {
                    if (!type.HasView)
                        continue;
                    sb.AppendLine($"_worldView.Create{type.Name}ViewModel(result);");
                }

                sb.EndBlock();
                sb.AppendLine("return result;");
                sb.EndBlock();
            }

            sb.AppendLine("public void Destroy(IEntity entity) => _worldState.ToDestroy.Add(entity);");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldView : IWorldView");
            sb.AppendLine("//TODO: implement IDisposable");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly IViewsProvider _viewsProvider;");
            foreach (var entity in Entities)
            foreach (var property in entity.GetPrefabsProperties())
                sb.AppendLine(
                    $"private readonly Dictionary<{entity.Name}ViewModel, {typeof(Template).FullName}> _views{entity.Name}{property} = new();");
            sb.BeginBlock("public WorldView(WorldState worldState, IViewsProvider viewsProvider)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_viewsProvider = viewsProvider;");
            sb.EndBlock();
            foreach (var entityInfo in Entities.Where(x => x.HasView))
            {
                sb.AppendLine(
                    $"private readonly Dictionary<{entityInfo.Name}, {entityInfo.Name}ViewModel> _viewModels{entityInfo.Name}Dictionary = new ();");
                sb.AppendLine(
                    $"private readonly List<{entityInfo.Name}ViewModel> _viewModels{entityInfo.Name}List = new ();");
                sb.AppendLine($"/* private readonly List<{entityInfo.Name}> _toRemove{entityInfo.Name} = new (); */");
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}ViewModel> AllOf{entityInfo.Name} => _viewModels{entityInfo.Name}List;");
                sb.BeginBlock($"public void Create{entityInfo.Name}ViewModel({entityInfo.Name} model)");
                sb.AppendLine(
                    $"if (_viewModels{entityInfo.Name}Dictionary.TryGetValue(model, out var viewModel)) return;");
                sb.AppendLine(
                    $"_viewModels{entityInfo.Name}Dictionary.Add(model, viewModel = new {entityInfo.Name}ViewModel(model));");
                sb.AppendLine($"_viewModels{entityInfo.Name}List.Add(viewModel);");
                foreach (var property in entityInfo.GetPrefabsProperties())
                {
                    sb.BeginBlock($"// Spawn views for {entityInfo.Name} by {property}");
                    sb.AppendLine(
                        $"var view = _viewsProvider.Spawn<{typeof(Template).FullName}>(model.{property});");
                    sb.AppendLine($"view.ViewModel = viewModel;");
                    sb.AppendLine($"_views{entityInfo.Name}{property}.Add(viewModel, view);");
                    sb.EndBlock();
                }

                sb.EndBlock();
                sb.BeginBlock($"public void Destroy{entityInfo.Name}ViewModel({entityInfo.Name} model)");

                sb.BeginBlock($"if (_viewModels{entityInfo.Name}Dictionary.Remove(model, out var viewModel))");
                sb.AppendLine($"_viewModels{entityInfo.Name}List.Remove(viewModel);");
                foreach (var property in entityInfo.GetPrefabsProperties())
                    sb.AppendLine(
                        $"if (_views{entityInfo.Name}{property}.Remove(viewModel, out var view)) _viewsProvider.Release(view);");
                sb.EndBlock();

                sb.EndBlock();
            }

            sb.AppendLine("/*");
            sb.BeginBlock("public void SyncViewModels()");
            foreach (var entityInfo in Entities.Where(x => x.HasView))
            {
                sb.BeginBlock($"//Sync {entityInfo.Name} view models");
                sb.AppendLine($"var models = _worldState.AllOf{entityInfo.Name};");
                sb.AppendLine($"_toRemove{entityInfo.Name}.Clear();");
                sb.BeginForEachIterationBlock("pair",
                    $"_viewModels{entityInfo.Name}Dictionary.Where(pair => !models.Contains(pair.Key))");
                sb.AppendLine($"_toRemove{entityInfo.Name}.Add(pair.Key);");
                sb.EndBlock();
                sb.BeginForIterationBlock("model", $"_toRemove{entityInfo.Name}");
                sb.AppendLine($"Destroy{entityInfo.Name}ViewModel(model);");
                sb.EndBlock();
                sb.BeginForIterationBlock("model", "models");
                sb.AppendLine($"Create{entityInfo.Name}ViewModel(model);");
                sb.EndBlock();
                sb.EndBlock();
            }

            sb.EndBlock();
            sb.AppendLine("*/");
            sb.AppendLine("/*");
            sb.BeginBlock($"public void DestroyViewModels(IEntity model)");
            foreach (var entityInfo in Entities.Where(x => x.HasView))
                sb.AppendLine(
                    $"if(model is {entityInfo.Name} val{entityInfo.Name}) Destroy{entityInfo.Name}ViewModel(val{entityInfo.Name});");
            sb.EndBlock();
            sb.AppendLine("*/");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldSimulation : IWorldSimulation");
            sb.AppendLine("private readonly List<ISimSystem> _simSystems = new ();");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly WorldView _worldView;");

            string GetTimersFieldName(TimerData timer) =>
                $"{timer.Type.Name}{timer.Timer}Handlers".ConvertToCamelCaseFieldName();

            foreach (var timer in allTimers)
            {
                var timerField = GetTimersFieldName(timer);
                sb.AppendLine($"private readonly List<I{timer.Type.Name}{timer.Timer}Handler> {timerField} = new ();");
            }

            sb.BeginBlock("public WorldSimulation(WorldState worldState, WorldView worldView)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_worldView = worldView;");
            sb.EndBlock();
            foreach (var timer in allTimers)
            {
                var timerField = GetTimersFieldName(timer);
                sb.AppendLine(
                    $"public void AddTimerHandler(I{timer.Type.Name}{timer.Timer}Handler handler) => {timerField}.Add(handler);");
            }

            sb.BeginBlock("public void AddSystem(ISimSystem simSystem)");
            sb.AppendLine("_simSystems.Add(simSystem);");
            sb.EndBlock();
            sb.BeginBlock("public void Simulate(float dt)");
            sb.AppendLine("AdvanceTimers(dt);");
            sb.BeginForIterationBlock("simSystem", "_simSystems");
            sb.AppendLine("simSystem.Simulate(dt);");
            sb.EndBlock();
            sb.AppendLine("DestroyEntities();");
            sb.AppendLine("//_worldView.SyncViewModels();");
            sb.EndBlock();
            sb.BeginBlock("void AdvanceTimers(float dt)");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllTimers().ToList();
                if (args.Count == 0)
                    continue;
                sb.BeginBlock();
                sb.AppendLine($"var list = _worldState.AllOf{entityInfo.Name};");
                sb.BeginForIterationBlock("e", "list");
                sb.AppendLine($"e.AdvanceTimers(dt);");
                sb.EndBlock();
                sb.EndBlock();
            }

            foreach (var timer in allTimers)
            {
                sb.BeginBlock();
                sb.AppendLine($"var list = _worldState.AllOf{timer.Type.Name};");
                sb.BeginForIterationBlock("e", "list");
                sb.AppendLine($"if(!e.{timer.Timer}JustFinished) continue;");
                sb.BeginForIterationBlock("handler", GetTimersFieldName(timer), "hIndex");
                sb.AppendLine($"handler.On{timer.Type.Name}{timer.Timer}Finish(e);");
                sb.EndBlock();
                sb.EndBlock();
                sb.EndBlock();
            }

            sb.EndBlock();
            sb.BeginBlock("void DestroyEntities()");
            sb.BeginForEachIterationBlock("entity", "_worldState.ToDestroy");
            sb.AppendLine("Destroy(entity);");
            sb.EndBlock();
            sb.AppendLine("_worldState.ToDestroy.Clear();");
            sb.EndBlock();
            sb.BeginBlock($"void Destroy(IEntity entity)");
            sb.AppendLine($"_worldState.Entities.Remove(entity);");
            sb.BeginBlock($"switch (entity)");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                sb.BeginBlock($"case {entityInfo.Name} val{entityInfo.Name}:");
                sb.AppendLine("//Clean state cache");
                foreach (var entityBase in entityInfo.GetAllImplemented())
                    sb.AppendLine($"_worldState._allOf{entityBase.Name}.Remove(val{entityInfo.Name});");
                sb.AppendLine("//Clean views");
                foreach (var entityBase in entityInfo.GetAllImplemented().Where(x => x.HasView))
                    sb.AppendLine($"_worldView.Destroy{entityBase.Name}ViewModel(val{entityInfo.Name});");
                sb.AppendLine($"break;");
                sb.EndBlock();
            }

            sb.EndBlock();
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class ResourcesViewsProvider : IViewsProvider");
            sb.AppendLine(
                "public void Release<TView>(TView value) where TView : Component => UnityEngine.Object.Destroy(value.gameObject);");
            sb.AppendLine(
                "public TView Spawn<TView>(string prefabName) where TView : Component => UnityEngine.Object.Instantiate(Resources.Load<TView>(prefabName));");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class PoolViewsProvider : IViewsProvider");
            sb.AppendLine("private readonly Dictionary<object, IDisposable> _cache = new();");
            sb.AppendLine($"private readonly {typeof(IObjectsPool).FullName} _objectsPool;");
            sb.BeginBlock($"public PoolViewsProvider({typeof(IObjectsPool).FullName} objectsPool)");
            sb.AppendLine("_objectsPool = objectsPool;");
            sb.EndBlock();
            sb.BeginBlock("void IViewsProvider.Release<TView>(TView value)");
            sb.AppendLine("if(_cache.Remove(value, out var disposable)) disposable.Dispose();");
            sb.EndBlock();
            sb.BeginBlock("TView IViewsProvider.Spawn<TView>(string prefabName)");
            sb.AppendLine("var disposable = _objectsPool.Instantiate<TView>(prefabName);");
            sb.AppendLine("_cache.Add(disposable.Instance, disposable);");
            sb.AppendLine("return disposable.Instance;");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public enum ViewsSpawnType");
            sb.AppendLine("Custom,");
            sb.AppendLine("Resources,");
            sb.AppendLine("Pool");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public enum SimulationType");
            sb.AppendLine("None,");
            sb.AppendLine("Fixed,");
            sb.AppendLine("Floating");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class FloatingSimulationService : IDisposable");
            sb.AppendLine($"private readonly {typeof(SimulationSettings).FullName} _settings;");
            sb.AppendLine("private readonly IWorldSimulation _worldSimulation;");
            sb.AppendLine(
                "private readonly System.Threading.CancellationTokenSource _cancellationTokenSource = new();");
            sb.BeginBlock(
                $"public FloatingSimulationService({typeof(SimulationSettings).FullName} settings, IWorldSimulation worldSimulation)");
            sb.AppendLine("_settings = settings;");
            sb.AppendLine("_worldSimulation = worldSimulation;");
            sb.AppendLine(
                $"{typeof(AsyncExtension).FullName}.RunEveryUpdate(SimulateIteration, _cancellationTokenSource.Token);");
            sb.EndBlock();
            sb.BeginBlock("void SimulateIteration()");
            sb.BeginBlock("if (_settings.IsSimulationPaused)");
            sb.AppendLine("return;");
            sb.EndBlock();
            sb.AppendLine("var dt = _settings.SimulationSpeed * UnityEngine.Time.deltaTime;");
            sb.AppendLine("_worldSimulation.Simulate(dt);");
            sb.EndBlock();
            sb.BeginBlock("public void Dispose()");
            sb.AppendLine("_cancellationTokenSource.Cancel();");
            sb.AppendLine("_cancellationTokenSource.Dispose();");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class SimulationService : IDisposable");
            sb.AppendLine($"private readonly {typeof(SimulationSettings).FullName} _settings;");
            sb.AppendLine("private readonly IWorldSimulation _worldSimulation;");
            sb.AppendLine(
                "private readonly System.Threading.CancellationTokenSource _cancellationTokenSource = new();");
            sb.AppendLine("private float _simTime;");
            sb.BeginBlock(
                $"public SimulationService({typeof(SimulationSettings).FullName} settings, IWorldSimulation worldSimulation)");
            sb.AppendLine("_settings = settings;");
            sb.AppendLine("_worldSimulation = worldSimulation;");
            sb.AppendLine(
                $"{typeof(AsyncExtension).FullName}.RunEveryUpdate(SimulateIteration, _cancellationTokenSource.Token);");
            sb.EndBlock();
            sb.BeginBlock("void SimulateIteration()");
            sb.BeginBlock("if (_settings.IsSimulationPaused)");
            sb.AppendLine("_simTime = 0;");
            sb.AppendLine("return;");
            sb.EndBlock();
            sb.AppendLine("var dt = _settings.SimTickTime;");
            sb.AppendLine("_simTime += _settings.SimulationSpeed * UnityEngine.Time.deltaTime;");
            sb.BeginBlock("while (_simTime >= dt)");
            sb.AppendLine("_simTime -= dt;");
            sb.AppendLine("_worldSimulation.Simulate(dt);");
            sb.EndBlock();
            sb.EndBlock();
            sb.BeginBlock("public void Dispose()");
            sb.AppendLine("_cancellationTokenSource.Cancel();");
            sb.AppendLine("_cancellationTokenSource.Dispose();");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock($"public class WorldLibrary : {typeof(ILibrary).FullName}");
            sb.AppendLine("private readonly SimulationType _simulation;");
            sb.AppendLine("private readonly ViewsSpawnType _viewsHandlingType;");
            sb.BeginBlock("public WorldLibrary(SimulationType simulation, ViewsSpawnType viewsHandlingType)");
            sb.AppendLine("_simulation = simulation;");
            sb.AppendLine("_viewsHandlingType = viewsHandlingType;");
            sb.EndBlock();
            sb.BeginBlock($"public void Register({typeof(IContainer).FullName} container)");
            sb.AppendLine("container.Register<WorldState>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldController>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldView>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldSimulation>().AsInterfacesAndSelf().SingleInstance();");
            sb.BeginBlock("switch (_viewsHandlingType)");
            sb.BeginBlock("case ViewsSpawnType.Resources:");
            sb.AppendLine("container.Register<ResourcesViewsProvider>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.BeginBlock("case ViewsSpawnType.Pool:");
            sb.AppendLine("container.Register<PoolViewsProvider>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.EndBlock();
            sb.BeginBlock("switch (_simulation)");
            sb.BeginBlock("case SimulationType.Fixed:");
            sb.AppendLine("container.Register<SimulationService>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.BeginBlock("case SimulationType.Floating:");
            sb.AppendLine("container.Register<FloatingSimulationService>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.EndBlock();
            sb.EndBlock();
            sb.EndBlock();
        }

        private void WriteInterfaces(FormatWriter sb, List<TimerData> allTimers)
        {
            sb.AppendLine("public interface IEntity { }");
            sb.AppendLine();


            sb.BeginBlock("public interface ISimSystem");
            sb.AppendLine("void Simulate(float dt);");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface ITimer");
            sb.AppendLine("float FullTime { get; }");
            sb.AppendLine("float TimeLeft { get; }");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IView<in TModel>");
            sb.AppendLine("void UpdateDate(TModel model);");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IViewsProvider");
            sb.AppendLine("void Release<TView>(TView value) where TView : Component;");
            sb.AppendLine("TView Spawn<TView>(string prefabName) where TView : Component;");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldView");
            foreach (var entityInfo in Entities.Where(x => x.HasView))
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}ViewModel> AllOf{entityInfo.Name} {{ get; }}");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldController");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllProperties().Where(x => x.IsRequired);
                var argsStr = string.Join(", ", args.Select(x => $"{x.Type} {x.Name.ConvertToUnityPropertyName()}"));
                sb.AppendLine($"{entityInfo.Name} Create{entityInfo.Name}({argsStr});");
            }

            sb.AppendLine($"void Destroy(IEntity entity);");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldState");
            sb.AppendLine($"IReadOnlyList<IEntity> All {{ get; }}");
            foreach (var entityInfo in Entities)
            {
                if (entityInfo is EntityInfo { IsSingleton: true })
                    sb.AppendLine($"public {entityInfo.Name} {entityInfo.Name} {{ get; }}");
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} {{ get; }}");
            }

            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldSimulation");
            sb.AppendLine("void AddSystem(ISimSystem simSystem);");
            foreach (var timer in allTimers)
                sb.AppendLine($"void AddTimerHandler(I{timer.Type.Name}{timer.Timer}Handler handler);");
            sb.AppendLine("void Simulate(float dt);");
            sb.EndBlock();
            sb.AppendLine();
        }

        class TimerData
        {
            public string Timer;
            public EntityBase Type;
        }

        private List<TimerData> GetAllTimers()
        {
            var allTimers = Entities.SelectMany(entityType =>
            {
                return entityType.Timers.Select(x =>
                    new TimerData
                    {
                        Timer = x,
                        Type = entityType
                    });
            }).ToList();
            return allTimers;
        }

        public EntityBase CreateEntity(string name)
        {
            var r = Entities.Find(x => x.Name == name);
            if (r == null)
                Entities.Add(r = new EntityInfo() { Name = name });
            return (EntityInfo)r;
        }

        public EntityBase CreateEntityInterface(string name)
        {
            var r = Entities.Find(x => x.Name == name);
            if (r == null)
                Entities.Add(r = new EntityInterface() { Name = name });
            return (EntityInterface)r;
        }

        public ConfigEntity GetConfig(string configId)
        {
            var r = Configs.Find(x => x.Name == configId);
            if (r == null)
                Configs.Add(r = new ConfigEntity() { Name = configId });
            return r;
        }

        public EventEntity CreateEvent(string eventName, params string[] args)
        {
            var r = new EventEntity { Name = eventName };
            r.Args.AddRange(args);
            Events.Add(r);
            return r;
        }

        public EventHandlerModel CreateEventHandler(EventEntity evToHandle)
        {
            var r = new EventHandlerModel(evToHandle);
            Profile.Handlers.Add(r);
            return r;
        }

        public WindowModelInfo GetWindow(string name)
        {
            var r = Windows.Find(x => x.Name == name);
            if (r == null)
                Windows.Add(r = new WindowModelInfo() { Name = name });
            return r;
        }

        public void Parse(string source) => WorldModelCompiler.Parse(this, source);

        public ItemEntity GetItem(string name)
        {
            var r = Profile.Items.Find(x => x.Name == name);
            if (r == default)
            {
                Profile.Items.Add(r = new ItemEntity() { Name = name });
                CreateFilter($"AllOf{r.Name}", r);
            }
            return r;
        }

        public ItemFilterModel CreateFilter(string name, ItemEntity itemEntity)
        {
            var r = new ItemFilterModel() { Entity = itemEntity, Name = name };
            Profile.Filters.Add(r);
            return r;
        }
    }

    public class WindowModelInfo
    {
        public string Name { get; set; }

        public List<InfoGetter> Bindings = new();
        private List<WindowHandler> Handlers = new();

        public string ClassName => $"{Name}Window";

        public void Write(FormatWriter sb)
        {
            sb.AppendLine($"[{typeof(BindingAttribute).FullName}]");
            sb.BeginBlock($"public partial class {ClassName} : ProjectWindow");
            foreach (var getter in Bindings)
                sb.AppendLine(
                    $"[{typeof(BindingAttribute).FullName}] public {getter.Type} {getter.Name} => {getter.Code};");
            sb.AppendLine();
            foreach (var handler in Handlers)
            {
                sb.BeginBlock($"[{typeof(BindingAttribute).FullName}] public async void {handler.Name}()");
                handler.Write(sb);
                sb.EndBlock();
            }

            sb.EndBlock();
        }

        public WindowModelInfo AddInfo(string type, string name, string code)
        {
            Bindings.Add(new InfoGetter()
            {
                Code = code,
                Name = name,
                Type = type
            });
            return this;
        }

        public string GetButtonEvent(string buttonName)
        {
            return $"On{buttonName}ButtonAt{Name}Clicked";
        }

        public WindowHandler AddHandler(string name)
        {
            var r = new WindowHandler() { Name = name };
            Handlers.Add(r);
            return r;
        }

        public WindowHandler DefineButton(string buttonName, EventEntity evType)
        {
            var r = AddHandler($"On{buttonName}Clicked");
            r.RaiseOp(evType);
            return r;
        }
    }

    public static class WMDExtensions
    {
        public static EntityBase AddProperty<T>(this EntityBase e, string name, bool isRequired = true) =>
            e.AddProperty(typeof(T).FullName, name, isRequired);

        public static EntityBase Inherit(this EntityBase e, params EntityBase[] parents)
        {
            foreach (var parent in parents)
                e.Inherit(parent);
            return e;
        }
    }
}