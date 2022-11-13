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
                {
                    sb.AppendLine($"//TODO: {system.Name}");
                }
            });
        }
    }
}