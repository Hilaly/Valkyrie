using System.Collections.Generic;
using System.Linq;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        internal static string ToFullName(this string typeName) => typeName.Replace("+", ".");
        internal static string Clean(this string typeName) => typeName.Replace(".", "").Replace("+", "");

        internal static string GetComponentFullName(this string componentName)
        {
            return $"{componentName}Component";
        }

        private static void WriteComponents(IWorldInfo worldInfo, FormatWriter sb)
        {
            sb.WriteRegion("Components", () =>
            {
                foreach (var component in worldInfo.GetComponents())
                    component.WriteComponent(sb);
            });
        }

        static void WriteComponent(this IComponentInfo info, FormatWriter sb)
        {
            var template = GetComponentTemplate(info);
            template.Write(info, sb);
        }

        private static IComponentTemplate GetComponentTemplate(IComponentInfo info)
        {
            if (info is NativeTypeEventArchetype archetype)
                return new EventComponentTemplate(archetype);

            if (!ComponentTemplates.TryGetValue(info.GetTypeName(), out var template))
                template = ComponentTemplates["default"];
            return template;
        }

        private static IComponentTemplate GetComponentTemplate(IArchetypeInfo archetypeInfo, IPropertyInfo info)
        {
            if (archetypeInfo is NativeTypeEventArchetype archetype)
                return new EventComponentTemplate(archetype);

            if (!ComponentTemplates.TryGetValue(info.GetTypeName(), out var template))
                template = ComponentTemplates["default"];
            return template;
        }
    }
}