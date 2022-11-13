using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        static void WriteGeneralClassesAndInterfaces(IWorldInfo worldInfo, FormatWriter sb)
        {
            sb.WriteRegion("General interfaces", () =>
            {
                sb.WriteBlock("public interface IWorldState", () => { });
            });
            sb.AppendLine();
            sb.WriteRegion("General interfaces implementation", () =>
            {
                sb.WriteBlock("class WorldState : IWorldState", () =>
                {
                    sb.AppendLine($"private readonly {typeof(IEcsWorld).FullName} _ecsWorld;");
                    sb.AppendLine();
                    
                    var t = "All";
                    WriteGroupField(t, sb);
                    WriteGroupBuffer(t, sb);

                    sb.WriteBlock($"public WorldState({typeof(IEcsWorld).FullName} ecsWorld)", () =>
                    {
                        sb.AppendLine("_ecsWorld = ecsWorld;");
                        sb.AppendLine();
                        WriteGroupConstructor(t, sb);
                    });
                    WriteGroupImpl(t, sb);
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

        static void WriteGroupConstructor(string name, FormatWriter sb)
        {
            sb.AppendLine($"_{name}Group = _ecsWorld.Groups.Build()");
            sb.AddTab();
            //TODO: filters
            sb.AppendLine(".Build();");
            sb.RemoveTab();
        }

        static void WriteGroupDefine(string name, FormatWriter sb)
        {
            sb.AppendLine($"public IReadOnlyList<{typeof(EcsEntity).FullName}> GetAllOf_{name}();");
        }

        static void WriteGroupImpl(string name, FormatWriter sb)
        {
            sb.AppendLine($"public IReadOnlyList<{typeof(EcsEntity).FullName}> GetAllOf_{name}() => _{name}Group.GetEntities(_{name}Buffer);");
        }
    }
}