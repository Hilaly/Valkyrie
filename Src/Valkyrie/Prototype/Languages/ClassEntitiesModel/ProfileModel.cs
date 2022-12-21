using System.Collections.Generic;
using System.Numerics;
using System.Threading.Tasks;
using Valkyrie.Di;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Meta.Configs;
using Valkyrie.Meta.Inventory;
using Valkyrie.Tools;

namespace Valkyrie
{
    public class ProfileModel
    {
        public HashSet<string> Counters = new();
        public List<EventHandlerModel> Handlers = new();
        public List<ItemType> Items = new();
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
                item.WriteInventoryClass(sb);
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
}