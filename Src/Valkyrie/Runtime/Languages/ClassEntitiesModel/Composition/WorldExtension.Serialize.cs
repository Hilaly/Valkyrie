using System.Linq;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        static void WriteGeneralClassesAndInterfaces(IWorldInfo worldInfo, FormatWriter sb)
        {
            var archetypes = worldInfo.GetArchetypes();

            sb.WriteRegion("General interfaces", () =>
            {
                var additional = "";
                if (archetypes.Any())
                    additional = " : \n\t\t" + string.Join(", \n\t\t",
                        archetypes.Select(x => $"{typeof(IStateFilter<>).Namespace}.IStateFilter<{x.Name}>"));

                sb.WriteBlock($"public interface IWorldState{additional}", () =>
                {
                    foreach (var archetype in archetypes)
                    {
                        if (archetype is NativeTypeEventArchetype)
                        {
                            sb.AppendLine($"//TODO: void Clear{archetype.Name.Clean()}();");
                        }
                    }
                });
            });
            sb.AppendLine();
            sb.WriteRegion("General interfaces implementation", () =>
            {
                sb.WriteBlock("class WorldState : IWorldState", () =>
                {
                    sb.AppendLine($"private readonly {typeof(IEcsWorld).FullName} _ecsWorld;");
                    sb.AppendLine();

                    foreach (var archetype in archetypes)
                    {
                        var structName = archetype.Name.Clean();
                        sb.AppendLine(
                            $"private readonly {typeof(GroupConverter<,>).Namespace}.GroupConverter<{structName}, {archetype.Name}> _{structName}Converter;");
                    }

                    sb.WriteBlock($"public WorldState({typeof(IEcsWorld).FullName} ecsWorld)", () =>
                    {
                        sb.AppendLine("_ecsWorld = ecsWorld;");
                        sb.AppendLine();
                        foreach (var archetype in archetypes)
                        {
                            var structName = archetype.Name.Clean();
                            WriteGroupConstructor(structName, archetype, sb);
                        }
                    });
                    sb.AppendLine();
                    foreach (var archetype in archetypes)
                    {
                        var structName = archetype.Name.Clean();
                        sb.AppendLine($"IReadOnlyList<{archetype.Name}> IStateFilter<{archetype.Name}>.GetAll() => _{structName}Converter.AsConverted();");
                        if (archetype is NativeTypeEventArchetype)
                            sb.AppendLine(
                                $"public void Clear{structName}() => _{structName}Converter.AsEntities().ForEach(x => x.Destroy());");
                    }
                });
            });
        }

        static void WriteGroupConstructor(string name, IArchetypeFilter filter, FormatWriter sb)
        {
            sb.AppendLine($"_{name}Converter = new (");
            sb.AddTab();
            sb.AppendLine($"_ecsWorld.Groups.Build()");
            if (filter != null)
            {
                if (filter.Required.Any())
                    sb.AppendLine($".AllOf<{string.Join(", ", filter.Required.Select(GetComponentFullName))}>()");
                if (filter.Excluded.Any())
                    sb.AppendLine($".NotOf<{string.Join(", ", filter.Excluded.Select(GetComponentFullName))}>()");
            }

            sb.AppendLine(".Build() );");
            sb.RemoveTab();
        }
    }
}