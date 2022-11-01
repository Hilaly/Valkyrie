using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public class Feature
    {
        public List<BaseType> Types = new();

        public T Get<T>(string name) where T : BaseType => (T)Types.Find(x => x is T && x.Name == name);

        protected T GetOrCreate<T>(string name) where T : BaseType, new()
        {
            var r = Get<T>(name);
            if(r == null)
                Types.Add(r = new T {Name = name});
            return r;
        }

        public IReadOnlyList<T> Get<T>() where T : BaseType => Types.OfType<T>().ToList();

        public EntityType CreateEntity(string name) => GetOrCreate<EntityType>(name);
        public ConfigType CreateConfig(string name) => GetOrCreate<ConfigType>(name);
        public ItemType CreateItem(string name) => GetOrCreate<ItemType>(name);
    }

    public class WorldModelInfo : Feature
    {
        public string Namespace = nameof(WorldModelInfo);

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
            foreach (var entity in Get<ConfigType>())
                entity.WriteConfigClass(sb);
        }

        private bool IsEntityInterface(BaseType baseType)
        {
            if (baseType is EntityType entityType)
                return Get<EntityType>().Any(x => x.BaseTypes.Contains(entityType));
            return false;
        }

        private bool IsEntityClass(BaseType baseType)
        {
            if (baseType is EntityType entityType)
                return Get<EntityType>().All(x => !x.BaseTypes.Contains(entityType));
            return false;
        }

        private void WriteEntities(FormatWriter sb)
        {
            foreach (var entityType in Get<EntityType>())
            {
                if(IsEntityClass(entityType))
                    entityType.WriteTypeClass(sb);
                else if (IsEntityInterface(entityType))
                    entityType.WriteTypeInterface(sb);
                else
                    throw new Exception($"Can not determine what is this entity is");
            }

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
            foreach (var entityInfo in Get<EntityType>())
            {
                sb.AppendLine($"public readonly List<{entityInfo.Name}> _allOf{entityInfo.Name} = new();");
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} => _allOf{entityInfo.Name}; // Entities.OfType<{entityInfo.Name}>().ToList();");
            }

            foreach (var entityInfo in Get<EntityType>().Where(x => x.IsSingleton))
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
            foreach (var entityInfo in Get<EntityType>().Where(IsEntityClass))
            {
                var allProperties = entityInfo.GetAllProperties(true).Where(x => x.IsRequired).OfType<IMember>();
                var allConfigs = entityInfo.GetAllConfigs();
                var args = allProperties.Union(allConfigs).ToList();
                var argsStr = string.Join(", ", args.Select(x => $"{x.GetMemberType()} {x.Name.ConvertToUnityPropertyName()}"));
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
            foreach (var entity in Get<EntityType>())
            foreach (var property in entity.GetPrefabsProperties())
                sb.AppendLine($"private readonly Dictionary<{entity.Name}ViewModel, {typeof(Template).FullName}> _views{entity.Name}{property.PropertyName} = new();");
            sb.BeginBlock("public WorldView(WorldState worldState, IViewsProvider viewsProvider)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_viewsProvider = viewsProvider;");
            sb.EndBlock();
            foreach (var entityInfo in Get<EntityType>().Where(x => x.HasView))
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
                    sb.BeginBlock($"// Spawn views for {entityInfo.Name} by {property.PropertyName}");
                    sb.AppendLine($"{typeof(Debug).FullName}.Assert(!string.IsNullOrEmpty(model.{property.PropertyName}), $\"{entityInfo.Name}.{property.PropertyName} is null or empty\");");
                    sb.AppendLine($"var view = _viewsProvider.Spawn<{typeof(Template).FullName}>(model.{property.PropertyName});");
                    sb.AppendLine($"view.ViewModel = viewModel;");
                    sb.AppendLine($"_views{entityInfo.Name}{property.PropertyName}.Add(viewModel, view);");
                    if(property.ViewName.NotNullOrEmpty())
                        sb.AppendLine($"model.{property.ViewName} = view.gameObject;");
                    sb.EndBlock();
                }

                sb.EndBlock();
                sb.BeginBlock($"public void Destroy{entityInfo.Name}ViewModel({entityInfo.Name} model)");

                sb.BeginBlock($"if (_viewModels{entityInfo.Name}Dictionary.Remove(model, out var viewModel))");
                sb.AppendLine($"_viewModels{entityInfo.Name}List.Remove(viewModel);");
                foreach (var property in entityInfo.GetPrefabsProperties())
                {
                    sb.BeginBlock(
                        $"if (_views{entityInfo.Name}{property.PropertyName}.Remove(viewModel, out var view))");
                    sb.AppendLine("_viewsProvider.Release(view);");
                    if(property.ViewName.NotNullOrEmpty())
                        sb.AppendLine($"model.{property.ViewName} = null;");
                    sb.EndBlock();
                }
                sb.EndBlock();

                sb.EndBlock();
            }

            sb.AppendLine("/*");
            sb.BeginBlock("public void SyncViewModels()");
            foreach (var entityInfo in Get<EntityType>().Where(x => x.HasView))
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
            foreach (var entityInfo in Get<EntityType>().Where(x => x.HasView))
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
            foreach (var entityInfo in Get<EntityType>().Where(IsEntityClass))
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
            foreach (var entityInfo in Get<EntityType>().Where(IsEntityClass))
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
            foreach (var entityInfo in Get<EntityType>().Where(x => x.HasView))
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}ViewModel> AllOf{entityInfo.Name} {{ get; }}");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldController");
            foreach (var entityInfo in Get<EntityType>().Where(IsEntityClass))
            {
                var allProperties = entityInfo.GetAllProperties(true).Where(x => x.IsRequired).OfType<IMember>();
                var allConfigs = entityInfo.GetAllConfigs();
                var args = allProperties.Union(allConfigs).ToList();
                var argsStr = string.Join(", ", args.Select(x => $"{x.GetMemberType()} {x.Name.ConvertToUnityPropertyName()}"));
                sb.AppendLine($"{entityInfo.Name} Create{entityInfo.Name}({argsStr});");
            }

            sb.AppendLine($"void Destroy(IEntity entity);");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldState");
            sb.AppendLine($"IReadOnlyList<IEntity> All {{ get; }}");
            foreach (var entityInfo in Get<EntityType>().Where(IsEntityClass))
            {
                if (entityInfo.IsSingleton)
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
            public BaseType Type;
        }

        private List<TimerData> GetAllTimers()
        {
            var allTimers = Get<EntityType>().SelectMany(entityType =>
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

        public ItemType GetItem(string name)
        {
            var r = Profile.Items.Find(x => x.Name == name);
            if (r == default)
            {
                Profile.Items.Add(r = new ItemType() { Name = name });
                CreateFilter($"AllOf{r.Name}", r);
            }
            return r;
        }

        public ItemFilterModel CreateFilter(string name, ItemType itemEntity)
        {
            var r = new ItemFilterModel() { Entity = itemEntity, Name = name };
            Profile.Filters.Add(r);
            return r;
        }
    }
}