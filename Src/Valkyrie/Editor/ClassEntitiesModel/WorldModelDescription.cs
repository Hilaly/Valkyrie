using System.Collections.Generic;
using System.Linq;
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
        protected readonly List<string> Timers = new();
        protected readonly List<InfoGetter> Infos = new();
        protected readonly List<MemberInfo> Configs = new();

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

        public IEnumerable<string> GetAllTimers()
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
            }

            foreach (var info in Configs) 
                sb.AppendLine($"public {info.Type} {info.Name} {{ get; set; }}");

            foreach (var info in Infos) 
                sb.AppendLine($"public {info.Type} {info.Name} {{ get; }}");

            sb.EndBlock();
        }
    }

    public class EntityInfo : EntityBase
    {
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
            foreach (var property in GetAllInfos())
                sb.AppendLine($"public {property.Type} {property.Name} => {property.Code};");
            
            var timers = GetAllTimers();
            foreach (var timer in timers)
            {
                sb.AppendLine($"private EntityTimer _{timer};");
                sb.AppendLine(
                    $"public ITimer {timer} => _{timer} is {{ TimeLeft: > 0 }} ? _{timer} : _{timer} = default;");
                sb.BeginBlock($"public void Start{timer}(float time)");
                sb.AppendLine($"if ({timer} != null) throw new Exception(\"Timer {timer} already exist\");");
                sb.AppendLine($"_{timer} = new EntityTimer(time);");
                sb.EndBlock();
                sb.AppendLine($"public void Stop{timer}() => _{timer} = default;");
            }

            if (timers.Any())
            {
                sb.BeginBlock("internal void AdvanceTimers(float dt)");
                foreach (var timer in timers) sb.AppendLine($"_{timer}?.Advance(dt);");
                sb.EndBlock();
            }

            sb.EndBlock();
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
        }

        private void WriteGeneral(FormatWriter sb)
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

            sb.BeginBlock("class EntityTimer : ITimer");
            sb.AppendLine("public float FullTime { get; }");
            sb.AppendLine("public float TimeLeft { get; private set; }");
            sb.BeginBlock("public EntityTimer(float time)");
            sb.AppendLine("FullTime = TimeLeft = time;");
            sb.EndBlock();
            sb.AppendLine("public void Advance(float dt) => TimeLeft -= dt;");
            sb.EndBlock();
            sb.AppendLine();

            sb.BeginBlock("public interface IWorldController");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllProperties().Where(x => x.IsRequired);
                var argsStr = string.Join(", ", args.Select(x => $"{x.Type} {x.Name}"));
                sb.AppendLine($"{entityInfo.Name} Create{entityInfo.Name}({argsStr});");
            }

            sb.AppendLine($"void Destroy(IEntity entity);");
            sb.EndBlock();

            sb.BeginBlock("public interface IWorldView");
            sb.AppendLine($"IReadOnlyList<IEntity> All {{ get; }}");
            foreach (var entityInfo in Entities)
                sb.AppendLine($"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} {{ get; }}");
            sb.EndBlock();

            sb.BeginBlock("public interface IWorldSimulation");
            sb.AppendLine("void AddSystem(ISimSystem simSystem);");
            sb.AppendLine("void Simulate(float dt);");
            sb.EndBlock();

            sb.BeginBlock("class WorldState");
            sb.AppendLine("public readonly List<IEntity> Entities = new();");
            sb.AppendLine("public readonly HashSet<IEntity> ToDestroy = new();");
            sb.EndBlock();
            sb.AppendLine();


            sb.BeginBlock("class WorldController : IWorldController");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.BeginBlock("public WorldController(WorldState worldState)");
            sb.AppendLine("_worldState = worldState;");
            sb.EndBlock();
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllProperties().Where(x => x.IsRequired);
                var argsStr = string.Join(", ", args.Select(x => $"{x.Type} {x.Name}"));
                sb.BeginBlock($"public {entityInfo.Name} Create{entityInfo.Name}({argsStr})");
                sb.BeginBlock($"var result = new {entityInfo.Name}");
                foreach (var propertyInfo in args)
                    sb.AppendLine($"{propertyInfo.Name} = {propertyInfo.Name},");
                sb.EndBlock();
                sb.AppendLine(";");
                sb.AppendLine("_worldState.Entities.Add(result);");
                sb.AppendLine("return result;");
                sb.EndBlock();
            }

            sb.AppendLine("public void Destroy(IEntity entity) => _worldState.ToDestroy.Add(entity);");
            sb.EndBlock();

            sb.BeginBlock("class WorldView : IWorldView");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.BeginBlock("public WorldView(WorldState worldState)");
            sb.AppendLine("_worldState = worldState;");
            sb.EndBlock();
            sb.AppendLine("public IReadOnlyList<IEntity> All => _worldState.Entities;");
            foreach (var entityInfo in Entities)
                sb.AppendLine(
                    $"public IReadOnlyList<{entityInfo.Name}> AllOf{entityInfo.Name} => _worldState.Entities.OfType<{entityInfo.Name}>().ToList();");
            sb.EndBlock();

            sb.BeginBlock("class WorldSimulation : IWorldSimulation");
            sb.AppendLine("private readonly List<ISimSystem> _simSystems = new ();");
            sb.AppendLine("private readonly IWorldView _worldView;");
            sb.AppendLine("private readonly WorldState _worldState;");
            sb.BeginBlock("public WorldSimulation(IWorldView worldView, WorldState worldState)");
            sb.AppendLine("_worldView = worldView;");
            sb.AppendLine("_worldState = worldState;");
            sb.EndBlock();
            sb.BeginBlock("public void AddSystem(ISimSystem simSystem)");
            sb.AppendLine("_simSystems.Add(simSystem);");
            sb.EndBlock();
            sb.BeginBlock("public void Simulate(float dt)");
            sb.AppendLine("AdvanceTimers(dt);");
            sb.AppendLine("foreach (var simSystem in _simSystems) simSystem.Simulate(dt);");
            sb.AppendLine("DestroyEntities();");
            sb.EndBlock();
            sb.BeginBlock("void AdvanceTimers(float dt)");
            foreach (var entityInfo in Entities.OfType<EntityInfo>())
            {
                var args = entityInfo.GetAllTimers().ToList();
                if (args.Count == 0)
                    continue;
                sb.AppendLine($"foreach (var e in _worldView.AllOf{entityInfo.Name}) e.AdvanceTimers(dt);");
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