using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Di;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        static void WriteSystems(IWorldInfo worldInfo, FormatWriter sb)
        {
            sb.WriteRegion("Systems", () =>
            {
                foreach (var system in worldInfo.GetSystems())
                    WriteSystemWrapper(system, sb);
            });
        }

        static string GetWrapperClassName(this ISystemInfo systemInfo)
        {
            var systemName = systemInfo.Name.ToFullName();
            var className = $"{systemName.Replace(".", "")}Wrapper";
            return className;
        }

        private static void WriteSystemWrapper(ISystemInfo systemInfo, FormatWriter sb)
        {
            var simParts = systemInfo.GetSimulation();
            var systemName = systemInfo.Name.ToFullName();
            var className = GetWrapperClassName(systemInfo);
            var header = $"class {className}";
            var baseTypes = new List<string>();
            if (systemInfo.NeedInit)
                baseTypes.Add(typeof(IEcsInitSystem).FullName);
            if (simParts.Any() || systemInfo.NeedSimulate)
                baseTypes.Add(typeof(IEcsSimulationSystem).FullName);
            if (baseTypes.Any())
                header += " : " + string.Join(", ", baseTypes);
            sb.WriteBlock(header, () =>
            {
                var typesReferences = simParts
                    .Where(x => x.Value != ArchetypeUsageInSystem.Event)
                    .Select(x => x.Key)
                    .ToHashSet();
                sb.AppendLine($"private readonly {systemName} _innerSystem;");
                if (simParts.Any(x => x.Value == ArchetypeUsageInSystem.Event))
                    sb.AppendLine($"[{typeof(InjectAttribute).FullName}] private readonly WorldState _worldState;");
                foreach (var archetypeInfo in typesReferences)
                {
                    sb.AppendLine(
                        $"[{typeof(InjectAttribute).FullName}] private readonly {typeof(IStateFilter<>).Namespace}.IStateFilter<{archetypeInfo.Name}> _{archetypeInfo.Name.Clean()}Filter;");
                }

                sb.AppendLine();
                sb.WriteBlock($"public {className}({systemName} innerSystem)",
                    () => { sb.AppendLine("_innerSystem = innerSystem;"); });
                if (systemInfo.NeedInit)
                    sb.AppendLine()
                        .WriteBlock($"void {typeof(IEcsInitSystem).FullName}.Init()",
                            () =>
                            {
                                sb.Profile($"{systemName}.Init", () => { sb.AppendLine("_innerSystem.Init();"); });
                            });
                if (simParts.Any() || systemInfo.NeedSimulate)
                {
                    sb.AppendLine()
                        .WriteBlock($"void {typeof(IEcsSimulationSystem).FullName}.Simulate(float dt)",
                            () =>
                            {
                                //Clear events
                                foreach (var (archetype, _) in simParts.Where(x =>
                                    x.Value == ArchetypeUsageInSystem.Event))
                                    sb.AppendLine($"_worldState.Clear{archetype.Name.Clean()}();");

                                //Simulate system
                                foreach (var (archetype, usage) in simParts.Where(x =>
                                    x.Value != ArchetypeUsageInSystem.Event))
                                {
                                    switch (usage)
                                    {
                                        case ArchetypeUsageInSystem.Multiple:
                                            sb.Profile($"{systemName}.Simulate(List<{archetype.Name}>,float)", () =>
                                                sb.AppendLine(
                                                    $"_innerSystem.Simulate(_{archetype.Name.Clean()}Filter.GetAll(), dt);"));
                                            break;
                                        case ArchetypeUsageInSystem.Single:
                                            sb.Profile($"{systemName}.Simulate({archetype.Name},float)",
                                                () => sb.AppendLine(
                                                    $"foreach (var e in _{archetype.Name.Clean()}Filter.GetAll()) _innerSystem.Simulate(e, dt);"));
                                            break;
                                        default:
                                            sb.AppendLine($"//TODO: unimplemented sim {archetype.Name} -> {usage}");
                                            break;
                                    }
                                }

                                //Simulate base
                                if (systemInfo.NeedSimulate)
                                    sb.Profile($"{systemName}.Simulate(float)", () =>
                                        sb.AppendLine($"_innerSystem.Simulate(dt);"));
                            });
                }
            });
        }

        internal static IEnumerable<KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>>
            CollectArchetypeUsagesFromSystem(this IWorldInfo worldInfo, Type type)
        {
            foreach (var implementedInterface in type.GetInterfaces())
            {
                if (!implementedInterface.IsConstructedGenericType)
                    continue;

                var genericType = implementedInterface.GetGenericTypeDefinition();
                if (genericType == typeof(IArchetypeSimSystem<>))
                {
                    var archetype = GetArchetypeInfo(worldInfo, implementedInterface, 0);
                    yield return new KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>(archetype,
                        ArchetypeUsageInSystem.Multiple);
                }
                else if (genericType == typeof(IArchetypeEntitySimSystem<>))
                {
                    var archetype = GetArchetypeInfo(worldInfo, implementedInterface, 0);
                    yield return new KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>(archetype,
                        ArchetypeUsageInSystem.Single);
                }
                else if (genericType == typeof(IEventSystem<>))
                {
                    var archetype = GetArchetypeInfo(worldInfo, implementedInterface, 0);
                    yield return new KeyValuePair<IArchetypeInfo, ArchetypeUsageInSystem>(archetype,
                        ArchetypeUsageInSystem.Event);
                }
            }
        }

        private static IArchetypeInfo GetArchetypeInfo(this IWorldInfo worldInfo, Type implementedInterface,
            int order)
        {
            var archetypeType = implementedInterface.GetGenericArguments()[order];
            worldInfo.RegisterArchetype(archetypeType);
            var archetypeName = archetypeType.FullName.ToFullName();
            var archetype = worldInfo.GetArchetypes().FirstOrDefault(x => x.Name == archetypeName);
            return archetype;
        }
    }
}