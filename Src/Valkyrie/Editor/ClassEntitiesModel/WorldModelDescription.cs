using System.Collections.Generic;
using System.Linq;
using Utils;
using Valkyrie.Language.Description.Utils;

namespace Editor.ClassEntitiesModel
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
            return this;
        }

        protected void WriteViewModels(FormatWriter sb)
        {
            if (this.GetPrefabsProperties().Any())
            {
                sb.BeginBlock($"[Binding] public partial class {Name}ViewModelMonoBehaviour : MonoBehaviour, IView<{Name}>");
                sb.AppendLine($"public void UpdateDate({Name} model) {{ }}");
                sb.EndBlock();
            }
        }
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

    public class WorldModelInfo
    {
        public string Namespace = "Test";

        public List<EntityBase> Entities = new List<EntityBase>();

        public override string ToString()
        {
            var sb = new FormatWriter();

            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Linq;");
            sb.AppendLine("using Valkyrie.Di;");
            sb.AppendLine("using UnityEngine;");
            sb.AppendLine("using Utils;");
            sb.AppendLine();

            var rootNamespace = Namespace;

            sb.BeginBlock($"namespace {rootNamespace}");
            WriteEntities(sb);
            sb.EndBlock();

            sb.AppendLine();

            sb.BeginBlock($"namespace {rootNamespace}");
            WriteGeneral(sb);
            sb.EndBlock();

            return sb.ToString();
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

            sb.BeginBlock("public interface IWorldState");
            sb.AppendLine($"IReadOnlyList<IEntity> All {{ get; }}");
            foreach (var entityInfo in Entities)
            {
                if (entityInfo is EntityInfo { IsSingleton: true })
                    sb.AppendLine($"public {entityInfo.Name} {entityInfo.Name} {{ get; }}");
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} {{ get; }}");
            }

            sb.EndBlock();

            sb.BeginBlock("public interface IWorldSimulation");
            sb.AppendLine("void AddSystem(ISimSystem simSystem);");
            foreach (var timer in allTimers)
                sb.AppendLine($"void AddTimerHandler(I{timer.Type.Name}{timer.Timer}Handler handler);");
            sb.AppendLine("void Simulate(float dt);");
            sb.EndBlock();

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
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} => Entities.OfType<{entityInfo.Name}>().ToList();");
            foreach (var entityInfo in Entities.Where(x => x is EntityInfo { IsSingleton: true }))
                sb.AppendLine(
                    $"public {entityInfo.Name} {entityInfo.Name} => ({entityInfo.Name})Entities.Find(x => x is {entityInfo.Name});");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldController : IWorldController");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.BeginBlock("public WorldController(WorldState worldState)");
            sb.AppendLine("_worldState = worldState;");
            sb.EndBlock();
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllProperties().Where(x => x.IsRequired).ToList();
                var argsStr = string.Join(", ", args.Select(x => $"{x.Type} {x.Name.ConvertToUnityPropertyName()}"));
                sb.BeginBlock($"public {entityInfo.Name} Create{entityInfo.Name}({argsStr})");
                if (entityInfo.IsSingleton)
                {
                    sb.AppendLine(
                        $"if(_worldState.Entities.Find(x => x is {entityInfo.Name}) != null) throw new Exception(\"{entityInfo.Name} already exists\");");
                }
                sb.BeginBlock($"var result = new {entityInfo.Name}");
                foreach (var propertyInfo in args)
                    sb.AppendLine($"{propertyInfo.Name} = {propertyInfo.Name.ConvertToUnityPropertyName()},");
                sb.EndBlock();
                sb.AppendLine(";");
                sb.AppendLine("_worldState.Entities.Add(result);");
                sb.AppendLine("return result;");
                sb.EndBlock();
            }

            sb.AppendLine("public void Destroy(IEntity entity) => _worldState.ToDestroy.Add(entity);");
            sb.EndBlock();

            sb.BeginBlock("class WorldView");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("[InjectOptional] private readonly IViewsProvider _viewsProvider;");
            foreach (var entity in Entities)
            {
                foreach (var property in entity.GetPrefabsProperties())
                {
                    sb.AppendLine($"private readonly Dictionary<{entity.Name}, {entity.Name}ViewModelMonoBehaviour> _views{entity.Name}{property} = new();");
                    sb.AppendLine($"private readonly List<{entity.Name}> _toRemove{entity.Name}{property} = new();");
                }
            }
            sb.BeginBlock("public WorldView(WorldState worldState)");
            sb.AppendLine("_worldState = worldState;");
            sb.EndBlock();

            sb.BeginBlock("public void SyncPrefabs()");
            sb.AppendLine("if(_viewsProvider == null) return;");
            foreach (var entity in Entities)
            foreach (var property in entity.GetPrefabsProperties())
                sb.AppendLine($"Sync{entity.Name}{property}();");
            sb.EndBlock();
            foreach (var entity in Entities)
            {
                foreach (var property in entity.GetPrefabsProperties())
                {
                    sb.BeginBlock($"void Sync{entity.Name}{property}()");
                    sb.AppendLine($"var models = _worldState.AllOf{entity.Name};");
                    sb.AppendLine($"_toRemove{entity.Name}{property}.Clear();");
                    sb.AppendLine($"foreach (var pair in _views{entity.Name}{property}.Where(pair => !models.Contains(pair.Key))) _toRemove{entity.Name}{property}.Add(pair.Key);");
                    sb.AppendLine($"foreach (var i in _toRemove{entity.Name}{property}) if (_views{entity.Name}{property}.Remove(i, out var view)) _viewsProvider.Release(view);");
                    sb.BeginBlock($"foreach (var model in models)");
                    sb.AppendLine($"if (!_views{entity.Name}{property}.TryGetValue(model, out var view)) _views{entity.Name}{property}.Add(model, view = _viewsProvider.Spawn<{entity.Name}ViewModelMonoBehaviour>(model.{property}));");
                    sb.AppendLine($"view.UpdateDate(model);");
                    sb.EndBlock();
                    sb.EndBlock();
                }
            }
            
            sb.EndBlock();

            sb.BeginBlock("class WorldSimulation : IWorldSimulation");
            sb.AppendLine("private readonly List<ISimSystem> _simSystems = new ();");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.AppendLine("private readonly WorldView _worldView;");
            foreach (var timer in allTimers)
            {
                sb.AppendLine($"private readonly List<I{timer.Type.Name}{timer.Timer}Handler> _{timer.Type.Name}{timer.Timer}Handlers = new ();");
            }
            sb.BeginBlock("public WorldSimulation(WorldState worldState, WorldView worldView)");
            sb.AppendLine("_worldState = worldState;");
            sb.AppendLine("_worldView = worldView;");
            sb.EndBlock();
            foreach (var timer in allTimers)
            {
                sb.AppendLine($"public void AddTimerHandler(I{timer.Type.Name}{timer.Timer}Handler handler) => _{timer.Type.Name}{timer.Timer}Handlers.Add(handler);");
            }
            sb.BeginBlock("public void AddSystem(ISimSystem simSystem)");
            sb.AppendLine("_simSystems.Add(simSystem);");
            sb.EndBlock();
            sb.BeginBlock("public void Simulate(float dt)");
            sb.AppendLine("AdvanceTimers(dt);");
            sb.AppendLine("foreach (var simSystem in _simSystems) simSystem.Simulate(dt);");
            sb.AppendLine("DestroyEntities();");
            sb.AppendLine("_worldView.SyncPrefabs();");
            sb.EndBlock();
            sb.BeginBlock("void AdvanceTimers(float dt)");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllTimers().ToList();
                if (args.Count == 0)
                    continue;
                sb.AppendLine($"foreach (var e in _worldState.AllOf{entityInfo.Name}) e.AdvanceTimers(dt);");
            }
            foreach (var timer in allTimers)
            {
                sb.BeginBlock($"foreach (var e in _worldState.AllOf{timer.Type.Name})");
                sb.AppendLine($"if(!e.{timer.Timer}JustFinished) continue;");
                sb.AppendLine(
                    $"foreach (var handler in _{timer.Type.Name}{timer.Timer}Handlers) handler.On{timer.Type.Name}{timer.Timer}Finish(e);");
                sb.EndBlock();
            }
            sb.EndBlock();
            sb.BeginBlock("void DestroyEntities()");
            sb.AppendLine("foreach (var entity in _worldState.ToDestroy) _worldState.Entities.Remove(entity);");
            sb.AppendLine("_worldState.ToDestroy.Clear();");
            sb.EndBlock();
            sb.EndBlock();


            sb.BeginBlock("public class WorldLibrary : ILibrary");
            sb.BeginBlock("public void Register(IContainer container)");
            sb.AppendLine("container.Register<WorldState>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldController>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldView>().AsInterfacesAndSelf().SingleInstance();");
            sb.AppendLine("container.Register<WorldSimulation>().AsInterfacesAndSelf().SingleInstance();");
            sb.EndBlock();
            sb.EndBlock();
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