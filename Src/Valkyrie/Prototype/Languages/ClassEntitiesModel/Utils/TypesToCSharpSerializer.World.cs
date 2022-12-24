using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;
using Valkyrie.Utils;
using Valkyrie.Utils.Pool;

namespace Valkyrie
{
    static partial class TypesToCSharpSerializer
    {
        public static void WriteToDirectory(WorldModelInfo world, string dirPath, string mainCsFile)
        {
            //1. Serialize all
            var rootNamespace = world.Namespace;
            var methods = new List<KeyValuePair<string, string>>
            {
                new(mainCsFile, ToString(world, false))
            };

            //Configs
            foreach (var configType in world.Get<ConfigType>())
                WriteToSeparateFile("Configs", $"{configType.Name}.cs",
                    sb => configType.WriteConfigClass(sb), rootNamespace, methods);
            WriteToSeparateFile("Configs", $"ProjectConfigService.cs",
                sb => WriteConfigService(world, sb), rootNamespace, methods);

            //Windows
            foreach (var window in world.Windows)
                WriteToSeparateFile("Windows", $"{window.ClassName}.cs", sb => window.WriteWindow(sb), rootNamespace,
                    methods);

            //Entities Views
            foreach (var entityType in world.Get<EntityType>().Where(x => world.IsEntityClass(x) && x.HasView))
                WriteToSeparateFile("Views", $"{entityType.Name}View.cs", sb => entityType.WriteView(sb), rootNamespace,
                    methods);

            //Events
            foreach (var e in world.Events)
                WriteToSeparateFile("Events", $"{e.ClassName}.cs", sb => e.Write(sb), rootNamespace, methods);
            WriteToSeparateFile("Events", "EventsHandler.cs", sb => world.Profile.WriteEvents(sb), rootNamespace,
                methods);

            //Profile
            WriteToSeparateFile("Profile", "PlayerProfile.cs", sb => world.Profile.Write(sb), rootNamespace, methods);

            //Simulation
            WriteToSeparateFile("Simulation", "Sim.cs", sb => WriteGeneral(world, sb), rootNamespace, methods);
            WriteToSeparateFile("Simulation", "Timers.cs", sb => WriteTimerHandlers(world, sb), rootNamespace, methods);
            foreach (var entityType in world.Get<EntityType>().Where(x => !x.IsNative))
                WriteToSeparateFile("Simulation", $"{entityType.Name}.cs", sb =>
                {
                    if (world.IsEntityClass(entityType))
                        entityType.WriteTypeClass(sb);
                    else if (world.IsEntityInterface(entityType))
                        entityType.WriteTypeInterface(sb);
                    else
                        throw new Exception($"Can not determine what is this entity is");
                }, rootNamespace, methods);

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
                UtilsExtensions.EnsureDirectoryExistsForFile(fullPath);
                Debug.Log($"[GENERATION]: writing to file {fullPath}");
                File.WriteAllText(fullPath, text);
            }

            Debug.Log($"[GENERATION]: SUCCESS in {dirPath}");
        }

        internal static void WriteToSeparateFile(string subDirectory, string fileName, Action<FormatWriter> writeCall,
            string rootNamespace, List<KeyValuePair<string, string>> methods) =>
            WriteToSeparateFileFull(Path.Combine(subDirectory, fileName), writeCall, rootNamespace, methods);

        private static void WriteToSeparateFileFull(string fileName, Action<FormatWriter> writeCall,
            string rootNamespace, List<KeyValuePair<string, string>> methods)
        {
            var sb = new FormatWriter();
            WriteFileStart(sb);
            sb.BeginBlock($"namespace {rootNamespace}");

            writeCall(sb);

            sb.EndBlock();
            methods.Add(new KeyValuePair<string, string>(fileName, sb.ToString()));
        }

        internal static string ToString(this WorldModelInfo world, bool includeMono)
        {
            var sb = new FormatWriter();

            WriteFileStart(sb);

            var rootNamespace = world.Namespace;

            sb.BeginBlock($"namespace {rootNamespace}");
            sb.AppendLine($"#region Ui");
            sb.AppendLine();
            if (includeMono)
                WriteUi(world, sb);
            WriteBaseClassesToImplement(world, sb);
            sb.AppendLine();
            sb.AppendLine($"#endregion //Ui");
            sb.EndBlock();

            return sb.ToString();
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


        static private void WriteUi(this WorldModelInfo world, FormatWriter sb)
        {
            foreach (var window in world.Windows)
                window.WriteWindow(sb);
        }

        static private void WriteBaseClassesToImplement(this WorldModelInfo world, FormatWriter sb)
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

        static private void WriteTimerHandlers(this WorldModelInfo world, FormatWriter sb)
        {
            var allTimers = world.GetAllTimers();
            foreach (var timer in allTimers)
            {
                sb.BeginBlock($"public interface I{timer.Item2.Name}{timer.Item1}Handler");
                sb.AppendLine(
                    $"void On{timer.Item2.Name}{timer.Item1}Finish({timer.Item2.Name} {timer.Item2.Name.ConvertToUnityPropertyName()});");
                sb.EndBlock();
            }
        }

        static private void WriteGeneral(this WorldModelInfo world, FormatWriter sb)
        {
            var allTimers = world.GetAllTimers();

            WriteInterfaces(world, sb, allTimers);

            sb.AppendLine(
                $"class WorldState : IWorldState, {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{typeof(IEntity).FullName}>");
            sb.AddTab();
            foreach (var entityInfo in world.Get<EntityType>())
                sb.AppendLine($", {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{entityInfo.Name}>");
            sb.RemoveTab();
            sb.BeginBlock();
            sb.AppendLine($"public readonly List<{typeof(IEntity).FullName}> Entities = new();");
            sb.AppendLine($"public readonly HashSet<{typeof(IEntity).FullName}> ToDestroy = new();");
            sb.AppendLine($"public IReadOnlyList<{typeof(IEntity).FullName}> All => Entities;");
            sb.AppendLine(
                $"IReadOnlyList<{typeof(IEntity).FullName}> {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{typeof(IEntity).FullName}>.GetAll() => All;");
            foreach (var entityInfo in world.Get<EntityType>())
            {
                sb.AppendLine($"public readonly List<{entityInfo.Name}> _allOf{entityInfo.GetFixedName()} = new();");
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.GetFixedName()} => _allOf{entityInfo.GetFixedName()}; // Entities.OfType<{entityInfo.Name}>().ToList();");
                sb.AppendLine(
                    $"IReadOnlyList<{entityInfo.Name}> {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{entityInfo.Name}>.GetAll() => AllOf{entityInfo.GetFixedName()};");
            }

            foreach (var entityInfo in world.Get<EntityType>().Where(x => x.IsSingleton))
                sb.AppendLine(
                    $"public {entityInfo.Name} {entityInfo.GetFixedName()} => ({entityInfo.Name})Entities.Find(x => x is {entityInfo.Name});");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldController : IWorldController");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly WorldView _worldView;");
            sb.BeginBlock("public WorldController(WorldState worldState, WorldView worldView)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_worldView = worldView;");
            sb.EndBlock();
            foreach (var entityInfo in world.Get<EntityType>().Where(world.IsEntityClass))
            {
                if (entityInfo.IsNative)
                    continue;
                
                //Create default ctor
                if(false)
                {
                    var allProperties = entityInfo.GetAllProperties(true).Where(x => x.IsRequired).OfType<IMember>();
                    var allConfigs = entityInfo.GetAllConfigs();
                    var args = allProperties.Union(allConfigs).ToList();
                    var argsStr = string.Join(", ", args.Select(x => $"{x.GetMemberType()} {x.Name.ConvertToUnityPropertyName()}"));
                    sb.WriteBlock($"public {entityInfo.Name} Create{entityInfo.GetFixedName()}({argsStr})", () =>
                    {
                        WriteEntityCreateMethod(sb, entityInfo, () =>
                        {
                            foreach (var propertyInfo in args)
                                sb.AppendLine($"{propertyInfo.Name} = {propertyInfo.Name.ConvertToUnityPropertyName()},");
                        });
                    });
                }
                foreach (var references in entityInfo.GetConstructors())
                {
                    var argsStr = CollectPropertiesForConstructor(entityInfo, references, out var propertyToInits);
                    sb.WriteBlock($"public {entityInfo.Name} Create{entityInfo.GetFixedName()}({argsStr})", () =>
                    {
                        WriteEntityCreateMethod(sb, entityInfo, () =>
                        {
                            foreach (var propertyToInit in propertyToInits)
                                sb.AppendLine($"{propertyToInit.Property.Name} = {propertyToInit.GetText()},");
                        });
                    });
                }
            }

            sb.WriteBlock($"public void Destroy({typeof(IEntity).FullName} entity)",
                () =>
                {
                    sb.AppendLine($"_worldState.ToDestroy.Add(entity);");
                    sb.AppendLine($"(entity as {typeof(IExtEntity).FullName})?.Destroy();");
                });
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldView : IWorldView");
            sb.AppendLine("//TODO: implement IDisposable");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly IViewsProvider _viewsProvider;");
            foreach (var entity in world.Get<EntityType>())
            foreach (var property in entity.GetPrefabsProperties())
                sb.AppendLine(
                    $"private readonly Dictionary<{entity.Name}ViewModel, {entity.Name}View> _views{entity.Name}{property.PropertyName} = new();");
            sb.BeginBlock("public WorldView(WorldState worldState, IViewsProvider viewsProvider)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_viewsProvider = viewsProvider;");
            sb.EndBlock();
            foreach (var entityInfo in world.Get<EntityType>().Where(x => x.HasView))
            {
                sb.AppendLine(
                    $"private readonly Dictionary<{entityInfo.Name}, {entityInfo.Name}ViewModel> _viewModels{entityInfo.Name}Dictionary = new ();");
                sb.AppendLine(
                    $"private readonly List<{entityInfo.Name}ViewModel> _viewModels{entityInfo.Name}List = new ();");
                sb.AppendLine($"/* private readonly List<{entityInfo.Name}> _toRemove{entityInfo.Name} = new (); */");
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}ViewModel> AllOf{entityInfo.GetFixedName()} => _viewModels{entityInfo.Name}List;");
                sb.BeginBlock($"public void Create{entityInfo.Name}ViewModel({entityInfo.Name} model)");
                sb.AppendLine(
                    $"if (_viewModels{entityInfo.Name}Dictionary.TryGetValue(model, out var viewModel)) return;");
                sb.AppendLine(
                    $"_viewModels{entityInfo.Name}Dictionary.Add(model, viewModel = new {entityInfo.Name}ViewModel(model));");
                sb.AppendLine($"_viewModels{entityInfo.Name}List.Add(viewModel);");
                foreach (var property in entityInfo.GetPrefabsProperties())
                {
                    sb.BeginBlock($"// Spawn views for {entityInfo.Name} by {property.PropertyName}");
                    sb.AppendLine(
                        $"{typeof(Debug).FullName}.Assert(!string.IsNullOrEmpty(model.{property.PropertyName}), $\"{entityInfo.Name}.{property.PropertyName} is null or empty\");");

                    var allProperties = entityInfo.GetAllProperties(true);
                    var positionProperty = allProperties.FirstOrDefault(x => x.Name == "Position");
                    if (positionProperty != null && positionProperty.TypeData.GetTypeName() == typeof(Vector3).FullName)
                    {
                        sb.AppendLine($"var startPosition = model.{positionProperty.Name};");
                        var rotationProperty = allProperties.FirstOrDefault(x =>
                            x.Name == "Rotation" && x.TypeData.GetTypeName() == typeof(Quaternion).FullName);
                        if (rotationProperty != null)
                            sb.AppendLine($"var startRotation = model.{rotationProperty.Name};");
                        else
                        {
                            var directionProperty = allProperties.FirstOrDefault(x =>
                                x.Name == "Direction" && x.TypeData.GetTypeName() == typeof(Vector3).FullName);
                            if (directionProperty != null)
                                sb.AppendLine(
                                    $"var startRotation = {typeof(Quaternion).FullName}.LookRotation(model.{directionProperty.Name}, Vector3.up);");
                            else
                                sb.AppendLine($"var startRotation = {typeof(Quaternion).FullName}.Identity;");
                        }

                        sb.AppendLine(
                            $"var view = _viewsProvider.Spawn<{entityInfo.Name}View>(model.{property.PropertyName}, startPosition, startRotation);");
                    }
                    else
                        sb.AppendLine(
                            $"var view = _viewsProvider.Spawn<{entityInfo.Name}View>(model.{property.PropertyName});");

                    sb.AppendLine($"view.Model = model;");
                    sb.AppendLine($"_views{entityInfo.Name}{property.PropertyName}.Add(viewModel, view);");
                    if (property.ViewName.NotNullOrEmpty())
                        sb.AppendLine($"model.{property.ViewName} = view;");
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
                    if (property.ViewName.NotNullOrEmpty())
                        sb.AppendLine($"model.{property.ViewName} = null;");
                    sb.EndBlock();
                }

                sb.EndBlock();

                sb.EndBlock();
            }

            sb.AppendLine("/*");
            sb.BeginBlock("public void SyncViewModels()");
            foreach (var entityInfo in world.Get<EntityType>().Where(x => x.HasView))
            {
                sb.BeginBlock($"//Sync {entityInfo.Name} view models");
                sb.AppendLine($"var models = _worldState.AllOf{entityInfo.GetFixedName()};");
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
            sb.BeginBlock($"public void DestroyViewModels({typeof(IEntity).FullName} model)");
            foreach (var entityInfo in world.Get<EntityType>().Where(x => x.HasView))
                sb.AppendLine(
                    $"if(model is {entityInfo.Name} val{entityInfo.Name}) Destroy{entityInfo.Name}ViewModel(val{entityInfo.Name});");
            sb.EndBlock();
            sb.AppendLine("*/");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldSimulation : IWorldSimulation");
            sb.AppendLine($"private readonly List<{typeof(ISimSystem).FullName}> _simSystems = new ();");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly WorldView _worldView;");

            string GetTimersFieldName((string, BaseType) timer) =>
                $"{timer.Item2.Name}{timer.Item1}Handlers".ConvertToCamelCaseFieldName();

            foreach (var timer in allTimers)
            {
                var timerField = GetTimersFieldName(timer);
                sb.AppendLine($"private readonly List<I{timer.Item2.Name}{timer.Item1}Handler> {timerField} = new ();");
            }

            sb.BeginBlock("public WorldSimulation(WorldState worldState, WorldView worldView)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_worldView = worldView;");
            sb.EndBlock();
            foreach (var timer in allTimers)
            {
                var timerField = GetTimersFieldName(timer);
                sb.AppendLine(
                    $"public void AddTimerHandler(I{timer.Item2.Name}{timer.Item1}Handler handler) => {timerField}.Add(handler);");
            }

            sb.BeginBlock($"public void AddSystem({typeof(ISimSystem).FullName} simSystem)");
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
            foreach (var entityInfo in world.Get<EntityType>().Where(world.IsEntityClass))
            {
                var args = entityInfo.GetAllTimers().ToList();
                if (args.Count == 0)
                    continue;
                sb.BeginBlock();
                sb.AppendLine($"var list = _worldState.AllOf{entityInfo.GetFixedName()};");
                sb.BeginForIterationBlock("e", "list");
                sb.AppendLine($"e.AdvanceTimers(dt);");
                sb.EndBlock();
                sb.EndBlock();
            }

            foreach (var timer in allTimers)
            {
                sb.BeginBlock();
                sb.AppendLine($"var list = _worldState.AllOf{timer.Item2.GetFixedName()};");
                sb.BeginForIterationBlock("e", "list");
                sb.AppendLine($"if(!e.{timer.Item1}JustFinished) continue;");
                sb.BeginForIterationBlock("handler", GetTimersFieldName(timer), "hIndex");
                sb.AppendLine($"handler.On{timer.Item2.Name}{timer.Item1}Finish(e);");
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
            sb.BeginBlock($"void Destroy({typeof(IEntity).FullName} entity)");
            sb.AppendLine($"_worldState.Entities.Remove(entity);");
            sb.BeginBlock($"switch (entity)");
            foreach (var entityInfo in world.Get<EntityType>().Where(world.IsEntityClass))
            {
                sb.BeginBlock($"case {entityInfo.Name} val{entityInfo.GetFixedName()}:");
                sb.AppendLine("//Clean state cache");
                foreach (var entityBase in entityInfo.GetAllImplemented())
                    sb.AppendLine(
                        $"_worldState._allOf{entityBase.GetFixedName()}.Remove(val{entityInfo.GetFixedName()});");
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


            /*
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
            sb.AppendLine("#if UNITY_EDITOR");
            sb.AppendLine(
                "Debug.Assert(disposable.Instance != null, $\"Couldn't find {typeof(TView).Name} component on {prefabName}\");");
            sb.AppendLine("#endif");
            sb.AppendLine("_cache.Add(disposable.Instance, disposable);");
            sb.AppendLine("return disposable.Instance;");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();
            */


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

            sb.BeginBlock($"class WorldInstaller : {typeof(IWorldLoader).FullName}");
            sb.AppendLine($"[{typeof(InjectAttribute).FullName}] IWorldSimulation _worldSimulation;");
            foreach (var systemType in world.RegisteredSystems)
                sb.AppendLine(
                    $"[{typeof(InjectAttribute).FullName}] {systemType.Key.FullName} _{systemType.Key.FullName.Replace(".", "")};");
            sb.AppendLine(
                $"private readonly List<KeyValuePair<{typeof(ISimSystem).FullName}, int>> _addedSystems = new();");
            sb.AppendLine();
            sb.WriteBlock(
                "public void AddSystem<T>(T simSystem, int order = SimulationOrder.Default) where T : ISimSystem",
                () =>
                {
                    sb.AppendLine(
                        $"_addedSystems.Add(new KeyValuePair<{typeof(ISimSystem).FullName}, int>(new {typeof(ProfileSystem<>).Namespace}.ProfileSystem<T>( simSystem ), order));");
                });
            sb.AppendLine();
            sb.BeginBlock($"public {typeof(Task).FullName} InstallSystems()");
            sb.BeginBlock($"var temp = new List<KeyValuePair<{typeof(ISimSystem).FullName}, int>>(_addedSystems)");
            foreach (var systemType in world.RegisteredSystems)
                sb.AppendLine($"new( new {typeof(ProfileSystem<>).Namespace}.ProfileSystem<{systemType.Key.FullName}>( _{systemType.Key.FullName.Replace(".", "")} ), {systemType.Value} ),");
            sb.EndBlock();
            sb.AppendLine(";");
            sb.AppendLine(
                "foreach (var simSystem in temp.OrderBy(x => x.Value).Select(x => x.Key)) _worldSimulation.AddSystem(simSystem);");
            sb.AppendLine($"return {typeof(Task).FullName}.{nameof(Task.CompletedTask)};");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine();

            sb.BeginBlock($"public class WorldLibrary : {typeof(ILibrary).FullName}");
            sb.AppendLine($"private readonly {typeof(SimulationType).FullName} _simulation;");
            sb.AppendLine($"private readonly {typeof(ViewsSpawnType).FullName} _viewsHandlingType;");
            sb.BeginBlock(
                $"public WorldLibrary({typeof(SimulationType).FullName} simulation, {typeof(ViewsSpawnType).FullName} viewsHandlingType)");
            sb.AppendLine("_simulation = simulation;");
            sb.AppendLine("_viewsHandlingType = viewsHandlingType;");
            sb.EndBlock();
            sb.BeginBlock($"public void Register({typeof(IContainer).FullName} container)");
            sb.AppendLine("container.Register<WorldInstaller>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldState>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldController>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldView>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldSimulation>().AsInterfacesAndSelf().SingleInstance();");
            sb.BeginBlock("switch (_viewsHandlingType)");
            sb.BeginBlock($"case {typeof(ViewsSpawnType).FullName}.Resources:");
            sb.AppendLine(
                $"container.Register<{typeof(ResourcesViewsProvider).FullName}>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.BeginBlock($"case {typeof(ViewsSpawnType).FullName}.Pool:");
            sb.AppendLine(
                $"container.Register<{typeof(PoolViewsProvider).FullName}>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.EndBlock();
            sb.BeginBlock("switch (_simulation)");
            sb.BeginBlock($"case {typeof(SimulationType).FullName}.Fixed:");
            sb.AppendLine("container.Register<SimulationService>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.BeginBlock($"case {typeof(SimulationType).FullName}.Floating:");
            sb.AppendLine(
                "container.Register<FloatingSimulationService>().AsInterfacesAndSelf().SingleInstance().NonLazy();");
            sb.AppendLine("break;");
            sb.EndBlock();
            sb.EndBlock();
            sb.AppendLine("RegisterSystems(container);");
            sb.EndBlock();

            sb.BeginBlock($"void RegisterSystems({typeof(IContainer).FullName} container)");
            foreach (var pair in world.RegisteredSystems)
                sb.AppendLine($"container.Register<{pair.Key.FullName}>().AsInterfacesAndSelf().SingleInstance();");
            sb.EndBlock();

            sb.EndBlock();
        }

        private static string CollectPropertiesForConstructor(EntityType entityInfo, IReadOnlyList<BaseType> references,
            out List<IPropertyToInit> propertyToInits)
        {
            var allProperties = entityInfo.GetAllProperties(true).ToList();
            var existMembers = references.SelectMany(x => x.GetAllProperties(true)
                    .Select(u => new KeyValuePair<BaseType, IMember>(x, u)))
                .Union(references.SelectMany(x => x.GetAllInfos(true)
                    .Select(u => new KeyValuePair<BaseType, IMember>(x, u)))).ToList();
            propertyToInits = allProperties.ConvertAll<IPropertyToInit>(prop =>
            {
                foreach (var existMember in existMembers)
                {
                    if (existMember.Value.Name != prop.Name)
                        continue;
                    if (existMember.Value.GetMemberType() != prop.GetMemberType())
                        continue;
                    return new ConfigProperty()
                    {
                        Property = prop,
                        ConfigMember = existMember.Value,
                        ConfigType = existMember.Key
                    };
                }

                if (prop.TypeData is RefTypeData { Type: ConfigType })
                {
                    var exCfg = references.FirstOrDefault(x => x.Name == prop.GetMemberType());
                    if (exCfg != null)
                        return new ConfigInstance() { Property = prop, ConfigType = exCfg };
                }
                
                return prop.IsRequired ? new ParametersProperty() { Property = prop } : null;
            });

            propertyToInits.RemoveAll(x => x == null);
            var args = propertyToInits.Where(x => x.Property.IsRequired && x is ParametersProperty);
            var argsStr = string.Join(", ",
                args.Select(x =>
                        $"{x.Property.GetMemberType()} {x.Property.Name.ConvertToUnityPropertyName()}")
                    .Union(references.Select(x => $"{x.Name} {x.GetFixedName().ConvertToUnityPropertyName()}")));
            return argsStr;
        }

        static void WriteEntityCreateMethod(FormatWriter sb, EntityType entityInfo, Action writeProperties)
        {
            CheckForSingleton(sb, entityInfo);
            sb.WriteBlock($"var result = new {entityInfo.Name}", writeProperties);
            sb.AppendLine(";");
            SpawnViews(sb, entityInfo);
        }

        private static void SpawnViews(FormatWriter sb, EntityType entityInfo)
        {
            sb.AppendLine("_worldState.Entities.Add(result);");
            sb.WriteBlock($"//Update Caches", () =>
            {
                foreach (var entityBase in entityInfo.GetAllImplemented())
                    sb.AppendLine($"_worldState._allOf{entityBase.GetFixedName()}.Add(result);");
            });
            sb.WriteBlock($"//Spawn views", () =>
            {
                foreach (var type in entityInfo.GetAllImplemented())
                {
                    if (!type.HasView)
                        continue;
                    sb.AppendLine($"_worldView.Create{type.Name}ViewModel(result);");
                }
            });
            sb.AppendLine("return result;");
        }

        private static void CheckForSingleton(FormatWriter sb, EntityType entityInfo)
        {
            if (entityInfo.IsSingleton)
                sb.AppendLine(
                    $"if(_worldState.Entities.Find(x => x is {entityInfo.Name}) != null) throw new Exception(\"{entityInfo.Name} already exists\");");
        }

        static private void WriteInterfaces(this WorldModelInfo world, FormatWriter sb,
            List<(string, BaseType)> allTimers)
        {
            sb.BeginBlock("public interface IWorldView");
            foreach (var entityInfo in world.Get<EntityType>().Where(x => x.HasView))
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}ViewModel> AllOf{entityInfo.GetFixedName()} {{ get; }}");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldController");
            foreach (var entityInfo in world.Get<EntityType>().Where(world.IsEntityClass))
            {
                if (entityInfo.IsNative)
                    continue;

                foreach (var references in entityInfo.GetConstructors())
                {
                    var argsStr = CollectPropertiesForConstructor(entityInfo, references, out var propertyToInits);
                    sb.AppendLine($"{entityInfo.Name} Create{entityInfo.GetFixedName()}({argsStr});");
                }
            }

            sb.AppendLine($"void Destroy({typeof(IEntity).FullName} entity);");
            sb.EndBlock();
            sb.AppendLine();


            sb.AppendLine(
                $"public interface IWorldState : {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{typeof(IEntity).FullName}>");
            sb.AddTab();
            foreach (var entityInfo in world.Get<EntityType>())
                sb.AppendLine($", {typeof(IWorldFilter<>).Namespace}.IWorldFilter<{entityInfo.Name}>");
            sb.RemoveTab();
            sb.BeginBlock();
            sb.AppendLine($"IReadOnlyList<{typeof(IEntity).FullName}> All {{ get; }}");
            foreach (var entityInfo in world.Get<EntityType>())
            {
                if (entityInfo.IsSingleton)
                    sb.AppendLine($"public {entityInfo.Name} {entityInfo.GetFixedName()} {{ get; }}");
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.GetFixedName()} {{ get; }}");
            }

            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("public interface IWorldSimulation");
            sb.AppendLine($"void AddSystem({typeof(ISimSystem).FullName} simSystem);");
            foreach (var timer in allTimers)
                sb.AppendLine($"void AddTimerHandler(I{timer.Item2.Name}{timer.Item1}Handler handler);");
            sb.AppendLine("void Simulate(float dt);");
            sb.EndBlock();
            sb.AppendLine();
        }
    }
}