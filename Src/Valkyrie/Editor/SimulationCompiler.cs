using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor.Callbacks;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Ecs;

namespace Valkyrie.Editor
{
    public static class SimulationCompiler
    {
        static List<Type> GetAllSubTypes(this Type aBaseClass, Func<Type, bool> where)
        {
            var result = new List<Type>
            {
                aBaseClass
            };
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var assemblyTypes = assembly.GetTypes();
                    var selectedTypes = assemblyTypes
                        .Where(typ => typ.IsSubclassOf(aBaseClass) || aBaseClass.IsAssignableFrom(typ)).ToArray();
                    result.AddRange(selectedTypes);
                }
                catch
                {
                    //Do nothing if we got to assembly that probably not from this project
                }
            }

            return where != null ? result.Where(where).ToList() : result;
        }

        class Writer : IDisposable
        {
            private string _startStr;
            private readonly StreamWriter _fs;

            public Writer(string filename)
            {
                _fs = new StreamWriter(filename);
            }

            public void Decrease() => _startStr = _startStr.Substring(0, _startStr.Length - 1);
            public void Increase() => _startStr += "\t";

            public void Write(string msg) => _fs.WriteLine($"{_startStr}{msg}");
            public void Write() => _fs.WriteLine();

            public void StartNamespace(string namespaceName)
            {
                Write($"namespace {namespaceName}");
                Write("{");
                Increase();
            }

            public void EndNamespace()
            {
                Decrease();
                Write("}");
            }

            public void StartClass(string className, params string[] parents)
            {
                Write(
                    $"public partial class {className}{(parents.Length > 0 ? $" : {string.Join(", ", parents)}" : string.Empty)}");
                Write("{");
                Increase();
            }

            public void EndClass() => EndNamespace();

            public void StartRegion(string rg)
            {
                Write($"#region {rg}");
                Write();
            }

            public void EndRegion(string rg = null)
            {
                Write();
                Write($"#endregion{(!string.IsNullOrEmpty(rg) ? $" //{rg}" : string.Empty)}");
            }

            public void Dispose()
            {
                _fs?.Dispose();
            }
        }

        internal const string MonoTypeName = "GameObjectState";

        private static readonly string[] UsedNamespaces =
        {
            "System",
            "System.Collections.Generic",
            "UnityEngine",
            "Valkyrie.Di",
            "Valkyrie.Ecs"
        };

        [DidReloadScripts]
        [UnityEditor.MenuItem("Valkyrie/Ecs/Generate EcsGameState")]
        internal static void Recompile()
        {
            if (!MonoBehaviourCompilationPreferences.MonoSimulationCompilationEnabled)
                return;
            Debug.Log($"Valkyrie: regenerate components starting");
            var components = typeof(IComponent).GetAllSubTypes(x =>
                !x.IsAbstract && x.IsClass && typeof(MonoBehaviour).IsAssignableFrom(x)
                && (x.GetCustomAttribute<ObsoleteAttribute>() == null ||
                    x.GetCustomAttribute<ObsoleteAttribute>().IsError == false));
            var systems = typeof(ISystem).GetAllSubTypes(x =>
                !x.IsAbstract && x.IsClass
                              && (x.GetCustomAttribute<ObsoleteAttribute>() == null ||
                                  x.GetCustomAttribute<ObsoleteAttribute>().IsError == false)
            );
            if (components.Count > 0 || systems.Count > 0)
            {
                var targetPath = MonoBehaviourCompilationPreferences.MonoSimulationCompilationPath;
                if (!Path.HasExtension(targetPath))
                    targetPath += ".cs";
                var dir = Path.GetDirectoryName(targetPath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);
                CreateStateScript(targetPath, components);
                CreateSimulationServiceScript(targetPath, components, systems);
            }

            Debug.Log($"Valkyrie: regenerate components finished");
        }

        static IEnumerable<Type> CollectSystemsOfOrder(List<Type> systems, int order) => systems.Where(type =>
            typeof(ISimulationSystem).IsAssignableFrom(type) && type.GetOrder() == order);

        static void CreateSimulationServiceScript(string resultFilePath, List<Type> components, List<Type> systems)
        {
            var path = Path.GetDirectoryName(resultFilePath);
            var fileName = Path.Combine(path, "EcsSimulationService.cs");
            using var fb = new Writer(fileName);
            
            var orders = new Dictionary<int, List<Action>>();
            foreach (var type in systems.Where(type => typeof(ISimulationSystem).IsAssignableFrom(type)))
            {
                var order = type.GetOrder();
                if(orders.TryGetValue(order, out var list))
                    continue;
                orders.Add(order, list = new List<Action>());
                list.Add(() =>
                {
                    var strOrder = order.ToString().Replace("-", "_");
                    var varName = $"simulationSystemsOrder{strOrder}";
                    fb.Write($"// Simulation systems with order: {order}");
                    foreach (var systemType in CollectSystemsOfOrder(systems, order))
                        fb.Write($"// {systemType.FullName}");
                    fb.Write($"var {varName} = _simulationSystems.GetSimulationSystems({order});");
                    fb.Write($"if({varName} != null && {varName}.Count > 0)");
                    fb.Increase();
                    fb.Write($"for (int i = 0; i < {varName}.Count; i++)");
                    fb.Increase();
                    fb.Write($"{varName}[i].DoUpdate(dt);");
                    fb.Decrease();
                    fb.Decrease();
                });
            }
            foreach (var type in components.Where(type => typeof(ISimulationComponent).IsAssignableFrom(type)))
            {
                var order = type.GetOrder();
                if (!orders.TryGetValue(order, out var list))
                    orders.Add(order, list = new List<Action>());
                list.Add(() =>
                {
                    fb.Write($"// {type.Name} order: {order}");
                    fb.Write($"for (int i = 0; i < allObjects.Count; i++)");
                    fb.Write(
                        $"\tif (allObjects[i].gameObject.activeInHierarchy && allObjects[i].Has{type.ComponentName()})");
                    fb.Write($"\t\tallObjects[i].{type.ComponentName()}.DoUpdate(dt);");
                });
            }
            
            fb.Write("// <auto-generated>");
            fb.Write("//  Generated by Valkyrie.SimulationCompiler");
            fb.Write("// <auto-generated>");
            fb.Write();
            fb.Write($"using UnityEngine;");
            fb.Write();
            WriteComponentNamespace(fb);
            
            fb.StartClass($"EcsSimulationService", "MonoBehaviour");
            fb.Write($"[SerializeField] {typeof(SimulationSettings).FullName} _settings = new {typeof(SimulationSettings).FullName}();");
            fb.Write();
            fb.Write($"private readonly {typeof(GameSimulation).FullName} _simulationSystems = new {typeof(GameSimulation).FullName}();");
            fb.Write();
            fb.Write($"public {typeof(SimulationSettings).FullName} Settings => _settings;");
            fb.Write();
            fb.Write($"public void AddSystem({typeof(ISystem).FullName} system) => _simulationSystems.Add(system);");
            fb.Write();
            
            fb.Write($"private void Update()");
            fb.Write($"{{");
            fb.Increase();
            fb.Write("DoSimulation(UnityEngine.Time.deltaTime);");
            fb.Decrease();
            fb.Write($"}}");
            fb.Write();
            
            fb.Write($"private void FixedUpdate()");
            fb.Write($"{{");
            fb.Increase();
            fb.Write("//DoSimulation(UnityEngine.Time.fixedDeltaTime);");
            fb.Decrease();
            fb.Write($"}}");
            fb.Write();

            fb.Write($"private void DoSimulation(float unscaledTime)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"if(Settings.{nameof(SimulationSettings.IsSimulationPaused)}) return;");
            fb.Write();
            fb.Write($"var allObjects = {MonoTypeName}.GetAll();");
            fb.Write($"var dt = unscaledTime * Settings.{nameof(SimulationSettings.SimulationSpeed)};");
            fb.Write($"//{MonoTypeName}.DoUpdate(dt);");
            //fb.Write("/*");
            foreach (var pair in orders.OrderBy(x => x.Key))
            foreach (var action in pair.Value)
                action();
            //fb.Write("*/");
            fb.Decrease();
            fb.Write("}");
            fb.EndClass();

            fb.EndNamespace();
        }

        static void CreateStateScript(string resultFilePath, List<Type> components)
        {
            using var fb = new Writer(resultFilePath);
            fb.Write("// <auto-generated>");
            fb.Write("//  Generated by Valkyrie.SimulationCompiler");
            fb.Write("// <auto-generated>");
            fb.Write();

            WriteNamespaces(components, fb);
            fb.Write();
            WriteComponentNamespace(fb);

            fb.StartClass(MonoTypeName, $"CollectedMonoBehaviour<{MonoTypeName}>");
            FillClass(fb, components);
            fb.EndClass();

            fb.Write();

            fb.Write("/*");
            fb.StartClass($"{MonoTypeName}SimulationSystem", "ISimulationSystem");
            FillSimulationSystem(fb);
            fb.EndClass();
            fb.Write("*/");
            
            fb.EndNamespace();
        }

        private static void FillSimulationSystem(Writer fb)
        {
            fb.Write("public int Order => 0;");
            fb.Write();
            fb.Write($"public void DoUpdate(float dt) => {MonoTypeName}.DoUpdate(dt);");
        }

        private static string ComponentName(this Type type) => type.Name.Replace("Component", string.Empty);

        private static bool NeedInject(this Type type)
        {
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            if (fields.Length > 0)
                return true;
            var properties = type.GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            if (properties.Length > 0)
                return true;
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Where(u => u.GetCustomAttribute<InjectAttribute>(true) != null).ToArray();
            if (methods.Length > 0)
                return true;
            if (type.BaseType != null)
                return NeedInject(type.BaseType);
            return false;
        }

        private static int GetOrder(this Type type)
        {
            return type.GetCustomAttribute<OrderAttribute>()?.Order ?? 0;
        }

        private static void FillClass(Writer fb, List<Type> components)
        {
            FillProperties(fb);
            fb.Write();
            FillClassComponents(fb, components);
            fb.Write();
            FillClassMethods(fb, components);
            fb.Write();
            FillClassFields(fb, components);
        }

        private static void FillProperties(Writer fb)
        {
            fb.StartRegion("Properties");
            fb.Write($"public bool IsActive => gameObject.activeInHierarchy;");
            fb.EndRegion("Properties");
        }

        private static void FillClassFields(Writer fb, List<Type> components)
        {
            fb.StartRegion("Fields");
            fb.Write($"private IContainer _container;");
            fb.Write();
            foreach (var type in components)
                fb.Write($"private {type.FullName} _{type.ComponentName()};");
            fb.EndRegion("Fields");
        }

        private static void FillClassMethods(Writer fb, List<Type> components)
        {
            fb.StartRegion("Methods");

            fb.StartRegion("Components");

            foreach (var type in components)
            {
                fb.Write($"public {type.FullName} Get{type.ComponentName()}() => _{type.ComponentName()};");

                fb.Write(
                    $"public {type.FullName} GetOrCreate{type.ComponentName()}() => _{type.ComponentName()} == null ? Add{type.ComponentName()}() : _{type.ComponentName()};");

                fb.Write($"public {type.FullName} Add{type.ComponentName()}()");
                fb.Write("{");
                fb.Increase();
                fb.Write(
                    $"Debug.Assert(_{type.ComponentName()} == null, \"Component {type.ComponentName()} already exist.\", this);");
                fb.Write($"_{type.ComponentName()} = gameObject.AddComponent<{type.FullName}>();");
                if (type.NeedInject())
                    fb.Write($"Inject(_{type.ComponentName()});");
                fb.Write($"return _{type.ComponentName()};");
                fb.Decrease();
                fb.Write("}");

                fb.Write($"public void Remove{type.ComponentName()}()");
                fb.Write("{");
                fb.Increase();
                fb.Write(
                    $"Debug.Assert(_{type.ComponentName()} != null, \"Component {type.ComponentName()} does not exist.\", this);");
                fb.Write($"Destroy(_{type.ComponentName()});");
                fb.Write($"_{type.ComponentName()} = null;");
                fb.Decrease();
                fb.Write("}");
            }

            fb.EndRegion("Components");
            fb.Write();

            fb.StartRegion("Static");
            fb.Write($"public static void DoUpdate(float dt)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"var allObjects = Active;");
            foreach (var type in components.Where(type => typeof(ISimulationComponent).IsAssignableFrom(type))
                .OrderBy(type => type.GetOrder()))
            {
                fb.Write($"// {type.Name} order: {type.GetOrder()}");
                fb.Write($"for (int i = 0; i < allObjects.Count; i++)");
                fb.Write(
                    $"\tif (allObjects[i].gameObject.activeInHierarchy && allObjects[i]._{type.ComponentName()} != null)");
                fb.Write($"\t\tallObjects[i]._{type.ComponentName()}.DoUpdate(dt);");
            }

            fb.Decrease();
            fb.Write("}");
            fb.Write();

            fb.Write($"public static {MonoTypeName} CreateEntity(string name = null)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"var go = string.IsNullOrEmpty(name) ? new GameObject() : new GameObject(name);");
            fb.Write($"return RegisterEntity(go, name);");
            fb.Decrease();
            fb.Write("}");

            fb.Write($"public static {MonoTypeName} CreateEntity(GameObject prefab, string name = null)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"var go = Instantiate(prefab);");
            fb.Write($"return RegisterEntity(go, name);");
            fb.Decrease();
            fb.Write("}");

            fb.Write(
                $"public static {MonoTypeName} CreateEntity<T>(T prefab, string name = null) where T : Component => CreateEntity(prefab.gameObject, name);");

            fb.Write($"static {MonoTypeName} RegisterEntity(GameObject instance, string name)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"if (!string.IsNullOrEmpty(name) && instance.name != name) instance.name = name;");
            fb.Write($"var result = instance.GetComponent<{MonoTypeName}>();");
            fb.Write($"if (result == null)");
            fb.Write("{");
            fb.Increase();
            fb.Write($"result = instance.AddComponent<{MonoTypeName}>();");
            fb.Write($"result.InjectAll();");
            fb.Decrease();
            fb.Write("}");
            fb.Write($"return result;");
            fb.Decrease();
            fb.Write("}");

            fb.Write($"public static {MonoTypeName} RegisterEntity(GameObject instance)");
            fb.Write("{");
            fb.Increase();
            fb.Write(
                $"Debug.Assert(instance.GetComponent<{MonoTypeName}>() == null, $\"Object already registered as entity\", instance);");
            fb.Write($"return instance.AddComponent<{MonoTypeName}>();");
            fb.Decrease();
            fb.Write("}");

            fb.EndRegion("Static");

            fb.StartRegion("Unity events");

            fb.Write("protected override void Awake()");
            fb.Write("{");
            fb.Increase();
            fb.Write("CollectComponents();");
            fb.Write("base.Awake();");
            fb.Decrease();
            fb.Write("}");

            fb.Write("private void Inject(object o) => Container.Inject(o, this);");

            fb.Write("private void CollectComponents()");
            fb.Write("{");
            fb.Increase();
            foreach (var type in components)
                fb.Write($"_{type.ComponentName()} = GetComponent<{type.FullName}>();");
            fb.Decrease();
            fb.Write("}");

            fb.Write("private void InjectAll()");
            fb.Write("{");
            fb.Increase();
            foreach (var type in components)
                if (type.NeedInject())
                    fb.Write($"if(_{type.ComponentName()} != null) Inject(_{type.ComponentName()});");
            fb.Decrease();
            fb.Write("}");

            fb.EndRegion("Unity events");

            fb.StartRegion("Pooled behaviour");
            fb.Write("/*");
            fb.Write();
            
            fb.Write("private static ObjectsPool _objectsPool;");
            fb.Write("private static ObjectsPool ObjectsPool");
            fb.Write("{");
            fb.Increase();
            fb.Write("get");
            fb.Write("{");
            fb.Increase();
            fb.Write("if (_objectsPool == null)");
            fb.Write("{");
            fb.Increase();
            fb.Write("var statePool = new GameObject(\"GameObjectStatePool\");");
            fb.Write("DontDestroyOnLoad(statePool);");
            fb.Write(" _objectsPool = new ObjectsPool(statePool.transform);");
            fb.Decrease();
            fb.Write("}");

            fb.Write("return _objectsPool;");
            fb.Decrease();
            fb.Write("}");
            fb.Decrease();
            fb.Write("}");
            fb.Write();
            fb.Write("class PooledEntityBehaviour : MonoBehaviour");
            fb.Write("{");
            fb.Increase();
            fb.Write("public IPrefabPool Pool { get; set; }");
            fb.Decrease();
            fb.Write("}");
            fb.Write();
            fb.Write("public static GameObjectState CreateEntityInPool(GameObject prefab, string name = null)");
            fb.Write("{");
            fb.Increase();
            fb.Write("var pool = ObjectsPool.Get(prefab);");
            fb.Write("var go = pool.Get();");
            fb.Write(
                "var pooledBehaviour = go.GetComponent<PooledEntityBehaviour>() ?? go.AddComponent<PooledEntityBehaviour>();");
            fb.Write("pooledBehaviour.Pool = pool;");
            fb.Write("return RegisterEntity(go, name);");
            fb.Decrease();
            fb.Write("}");
            fb.Write();
            fb.Write(
                "public static GameObjectState CreateEntityInPool<T>(T prefab, string name = null) where T : Component => CreateEntityInPool(prefab.gameObject, name);");
            fb.Write();
            fb.Write("public static void DestroyEntity(GameObjectState goState)");
            fb.Write("{");
            fb.Increase();
            fb.Write("var pooledBehaviour = goState.GetComponent<PooledEntityBehaviour>();");
            fb.Write("if (pooledBehaviour != null && pooledBehaviour.Pool != null)");
            fb.Write("{");
            fb.Increase();
            fb.Write("pooledBehaviour.Pool.Release(goState.gameObject);");
            fb.Write("pooledBehaviour.Pool = null;");
            fb.Decrease();
            fb.Write("}");
            fb.Write("else");
            fb.Increase();
            fb.Write("Destroy(goState.gameObject);");
            fb.Decrease();
            fb.Decrease();
            fb.Write("}");
            fb.Write();
            fb.Write("public static void DestroyEntity(GameObjectState goState, float delay)");
            fb.Write("{");
            fb.Increase();
            fb.Write("var pooledBehaviour = goState.GetComponent<PooledEntityBehaviour>();");
            fb.Write("if (pooledBehaviour != null && pooledBehaviour.Pool != null)");
            fb.Increase();
            fb.Write("pooledBehaviour.Pool.Release(goState.gameObject, delay);");
            fb.Decrease();
            fb.Write("else");
            fb.Increase();
            fb.Write("Destroy(goState.gameObject, delay);");
            fb.Decrease();
            fb.Decrease();
            fb.Write("}");
            fb.Write();
            fb.Write("public void Destroy() => DestroyEntity(this);");

            fb.Write();
            fb.Write("*/");
            fb.EndRegion("Pooled behaviour");

            fb.EndRegion("Methods");
        }

        private static void FillClassComponents(Writer fb, List<Type> components)
        {
            fb.StartRegion("Components");

            fb.StartRegion("HAS properties");
            foreach (var componentType in components)
                fb.Write(
                    $"public bool Has{componentType.ComponentName()} => _{componentType.ComponentName()} != null;");
            fb.EndRegion("HAS properties");
            fb.Write();

            fb.StartRegion("GET/SET properties");
            foreach (var componentType in components)
                fb.Write(
                    $"public {componentType.FullName} {componentType.ComponentName()} => _{componentType.ComponentName()};");
            fb.EndRegion("GET/SET properties");

            fb.Write();
            fb.StartRegion("Other");
            fb.Write($"public IContainer Container => _container ??= gameObject.FindContainerInScene();");
            fb.EndRegion("Other");

            fb.EndRegion("Components");
        }

        static void WriteComponentNamespace(Writer fb)
        {
            var existType = typeof(MonoBehaviour).GetAllSubTypes(x => x.IsClass && !x.IsAbstract)
                .FirstOrDefault(x => x.Name == MonoTypeName);
            fb.StartNamespace(existType != null ? existType.Namespace : "Root");
        }

        private static void WriteNamespaces(List<Type> components, Writer fb)
        {
            var collectedNamespaces = new HashSet<string>(UsedNamespaces);
            foreach (var ns in collectedNamespaces)
                fb.Write($"using {ns};");
        }
    }
}