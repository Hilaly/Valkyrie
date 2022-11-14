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

            sb.WriteRegion("General interfaces", () => { sb.WriteBlock("public interface IWorldState", () => { }); });
            sb.AppendLine();
            sb.WriteRegion("General interfaces implementation", () =>
            {
                sb.WriteBlock("class WorldState : IWorldState", () =>
                {
                    sb.AppendLine($"private readonly {typeof(IEcsWorld).FullName} _ecsWorld;");
                    sb.AppendLine();

                    foreach (var archetype in archetypes) WriteGroupField(archetype.Name.Clean(), sb);
                    foreach (var archetype in archetypes) WriteGroupBuffer(archetype.Name.Clean(), sb);

                    sb.WriteBlock($"public WorldState({typeof(IEcsWorld).FullName} ecsWorld)", () =>
                    {
                        sb.AppendLine("_ecsWorld = ecsWorld;");
                        sb.AppendLine();
                        foreach (var archetype in archetypes)
                            WriteGroupConstructor(archetype.Name.Clean(), archetype, sb);
                    });
                    sb.AppendLine();
                    foreach (var archetype in archetypes) WriteGroupImpl(archetype.Name.Clean(), sb);
                });
            });
        }

        static void WriteGroupField(string name, FormatWriter sb)
        {
            sb.AppendLine($"private readonly {typeof(IEcsGroup).FullName} _{name}Group;");
        }

        static void WriteGroupBuffer(string name, FormatWriter sb)
        {
            sb.AppendLine($"private readonly List<{typeof(EcsEntity).FullName}> _{name}Buffer = new();");
        }

        static void WriteGroupConstructor(string name, IArchetypeFilter filter, FormatWriter sb)
        {
            sb.AppendLine($"_{name}Group = _ecsWorld.Groups.Build()");
            sb.AddTab();
            if (filter != null)
            {
                if (filter.Required.Any())
                    sb.AppendLine($".AllOf<{string.Join(", ", filter.Required.Select(GetComponentFullName))}>()");
                if (filter.Excluded.Any())
                    sb.AppendLine($".NotOf<{string.Join(", ", filter.Excluded.Select(GetComponentFullName))}>()");
            }

            sb.AppendLine(".Build();");
            sb.RemoveTab();
        }

        static void WriteGroupDefine(string name, FormatWriter sb)
        {
            sb.AppendLine($"public IReadOnlyList<{typeof(EcsEntity).FullName}> GetAllOf_{name}();");
        }

        static void WriteGroupImpl(string name, FormatWriter sb)
        {
            sb.AppendLine(
                $"public IReadOnlyList<{typeof(EcsEntity).FullName}> GetAllOf_{name}() => _{name}Group.GetEntities(_{name}Buffer);");
        }
    }
}