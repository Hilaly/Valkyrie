using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Languages.ClassEntitiesModel
{
    public class MemberInfo
    {
        public string Name;
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

    public class EventEntity
    {
        public string Name;
        public readonly List<string> Args = new List<string>();

        public string ClassName => $"{Name}Event";

        public void Write(FormatWriter sb)
        {
            var blockName = $"public sealed class {ClassName} : BaseEvent";
            if (Args.Any())
                blockName += $"<{Args.Join(", ")}>";
            sb.BeginBlock(blockName);
            sb.EndBlock();
        }
    }

    public class ConfigEntity
    {
        private const string BaseInterfaceName = "Configs.IConfigData";
        
        public string Name;
        protected readonly List<ConfigEntity> BaseTypes = new();
        protected readonly List<PropertyInfo> Properties = new();

        public void Write(FormatWriter sb)
        {
            var blockName = $"public class {Name} : ";
            if (BaseTypes.Count > 0)
                blockName += string.Join(", ", BaseTypes.Select(x => x.Name)) +  ", ";
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
    }

    public abstract class EntityBase
    {
        public string Name;
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
                sb.BeginBlock($"[Binding] public partial class {Name}ViewModel");
                sb.AppendLine($"public {Name} Model {{ get; }}");
                sb.BeginBlock($"public {Name}ViewModel({Name} model)");
                sb.AppendLine("Model = model;");
                sb.EndBlock();
                foreach (var property in GetAllProperties())
                    sb.AppendLine($"[Binding] public {property.Type} {property.Name} => Model.{property.Name};");
                foreach (var info in GetAllInfos())
                    sb.AppendLine($"[Binding] public {info.Type} {info.Name} => Model.{info.Name};");
                foreach (var info in GetAllSlots())
                    sb.AppendLine($"[Binding] public {info.Type} {info.Name} => Model.{info.Name};");
                foreach (var timer in GetAllTimers())
                {
                    sb.AppendLine($"[Binding] public bool HasTimer{timer} => Model.{timer} != null;");
                    sb.AppendLine($"[Binding] public float {timer}TimeLeft => Model.{timer}?.TimeLeft ?? 0f;");
                    sb.AppendLine($"[Binding] public float {timer}Time => {timer}FullTime - {timer}TimeLeft;");
                    sb.AppendLine($"[Binding] public float {timer}FullTime => Model.{timer}?.FullTime ?? 1f;");
                    sb.AppendLine(
                        $"[Binding] public float {timer}Progress => Mathf.Clamp01({timer}Time / {timer}FullTime);");
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
    
    class EventHandlerOperation
    {}

    class LogOperation : EventHandlerOperation
    {
        private readonly string _text;

        public LogOperation(string text)
        {
            _text = text;
        }
    }

    public class EventHandlerModel
    {
        public readonly EventEntity Event;
        private readonly string _uid;
        private List<EventHandlerOperation> _ops = new List<EventHandlerOperation>();

        public EventHandlerModel(EventEntity @event)
        {
            _uid = Guid.NewGuid().ToString().Replace("-", string.Empty);
            Event = @event;
        }

        public void Write(FormatWriter sb)
        {
            sb.BeginBlock($"System.Threading.Tasks.Task {GetMethodName()}({Event.ClassName} ev)");
            sb.AppendLine($"//TODO: here must be event handler for {Event.ClassName}");
            if (_ops.Count == 0)
                sb.AppendLine("return System.Threading.Tasks.Task.CompletedTask;");
            sb.EndBlock();
        }

        public EventHandlerModel LogOp(string text)
        {
            _ops.Add(new LogOperation(text));
            return this;
        }

        public string GetMethodName()
        {
            return $"On{Event.ClassName}Handle{_uid}";
        }
    }

    public class ProfileModel
    {
        public HashSet<string> Counters = new();
        public List<EventHandlerModel> Handlers = new();

        public ProfileModel AddCounter(string name)
        {
            Counters.Add(name);
            return this;
        }

        public void WriteEvents(FormatWriter sb)
        {
            sb.BeginBlock("class GeneratedEventsHandler : IDisposable");
            sb.AppendLine("private readonly Valkyrie.Di.CompositeDisposable _disposable = new();");
            sb.AppendLine("private readonly IPlayerProfile _profile;");
            sb.AppendLine("private readonly IEventSystem _events;");
            sb.AppendLine();
            sb.BeginBlock("public GeneratedEventsHandler(IPlayerProfile profile, IEventSystem eventSystem)");
            sb.AppendLine("_profile = profile;");
            sb.AppendLine("_events = eventSystem;");
            foreach (var handler in Handlers)
            {
                sb.AppendLine($"_disposable.Add(_events.Subscribe<{handler.Event.ClassName}>({handler.GetMethodName()}));");
            }
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("public void Dispose() => _disposable.Dispose();");
            sb.AppendLine();
            sb.AppendLine("#region Handlers");
            sb.AppendLine();
            foreach (var handler in Handlers)
            {
                handler.Write(sb);
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //Handlers");
            sb.EndBlock();
        }

        public void Write(FormatWriter sb)
        {
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
            foreach (var counter in Counters)
            {
                sb.AppendLine($"public int {counter} {{ get; set; }}");
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //Counters");
            sb.AppendLine();
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock("class PlayerProfile : IPlayerProfile");
            sb.AppendLine("readonly Meta.Inventory.IWallet _wallet;");
            sb.AppendLine("readonly Meta.Inventory.IInventory _inventory;");
            //TODO: other references
            sb.AppendLine();
            sb.BeginBlock("public PlayerProfile(Meta.Inventory.IWallet wallet, Meta.Inventory.IInventory inventory)");
            sb.AppendLine("_wallet = wallet;");
            sb.AppendLine("_inventory = inventory;");
            sb.EndBlock();
            sb.AppendLine();
            sb.AppendLine("#region Counters");
            sb.AppendLine();
            foreach (var counter in Counters)
            {
                sb.BeginBlock($"public int {counter}");
                sb.AppendLine($"get => (int)_wallet.GetAmount(GeneratedConstants.{counter}Name);");
                sb.AppendLine($"set => _wallet.SetAmount(GeneratedConstants.{counter}Name, value);");
                sb.EndBlock();
            }
            sb.AppendLine();
            sb.AppendLine("#endregion //Counters");
            sb.AppendLine();
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock("public class MetaLibrary : ILibrary");
            sb.BeginBlock("public void Register(IContainer container)");
            sb.AppendLine("container.Register<PlayerProfile>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<GeneratedEventsHandler>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.EndBlock();
            sb.EndBlock();
        }
    }

    public class WorldModelInfo
    {
        public string Namespace = "Test";

        public List<EntityBase> Entities = new();
        public List<ConfigEntity> Configs = new();
        public List<EventEntity> Events = new();
        public ProfileModel Profile = new();

        public override string ToString()
        {
            var sb = new FormatWriter();

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Valkyrie.Di;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using Utils;");
            sb.AppendLine("using Valkyrie.Utils.Pool;");
            sb.AppendLine();

            var rootNamespace = Namespace;

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
                    $"private readonly Dictionary<{entity.Name}ViewModel, Valkyrie.MVVM.Bindings.Template> _views{entity.Name}{property} = new();");
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
                        $"var view = _viewsProvider.Spawn<Valkyrie.MVVM.Bindings.Template>(model.{property});");
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
            sb.AppendLine("private readonly Valkyrie.Utils.Pool.IObjectsPool _objectsPool;");
            sb.BeginBlock("public PoolViewsProvider(Valkyrie.Utils.Pool.IObjectsPool objectsPool)");
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


            sb.BeginBlock("class SimulationService : IDisposable");
            sb.AppendLine("private readonly Valkyrie.Ecs.SimulationSettings _settings;");
            sb.AppendLine("private readonly IWorldSimulation _worldSimulation;");
            sb.AppendLine(
                "private readonly System.Threading.CancellationTokenSource _cancellationTokenSource = new();");
            sb.AppendLine("private float _simTime;");
            sb.BeginBlock(
                "public SimulationService(Valkyrie.Ecs.SimulationSettings settings, IWorldSimulation worldSimulation)");
            sb.AppendLine("_settings = settings;");
            sb.AppendLine("_worldSimulation = worldSimulation;");
            sb.AppendLine("AsyncExtension.RunEveryUpdate(SimulateIteration, _cancellationTokenSource.Token);");
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


            sb.BeginBlock("public class WorldLibrary : ILibrary");
            sb.AppendLine("private readonly bool _autoSimulate;");
            sb.AppendLine("private readonly ViewsSpawnType _viewsHandlingType;");
            sb.BeginBlock("public WorldLibrary(bool autoSimulate, ViewsSpawnType viewsHandlingType)");
            sb.AppendLine("_autoSimulate = autoSimulate;");
            sb.AppendLine("_viewsHandlingType = viewsHandlingType;");
            sb.EndBlock();
            sb.BeginBlock("public void Register(IContainer container)");
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
            sb.BeginBlock("if (_autoSimulate)");
            sb.AppendLine("container.Register<SimulationService>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
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
            if(r == null)
                Configs.Add(r = new ConfigEntity() { Name = configId});
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