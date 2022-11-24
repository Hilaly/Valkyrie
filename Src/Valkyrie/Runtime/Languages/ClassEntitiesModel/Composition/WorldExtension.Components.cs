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

        private static readonly Dictionary<string, IComponentTemplate> ComponentTemplates = new()
        {
            {
                typeof(bool).FullName, new ComponentTemplate()
                {
                    Getters = new List<string>()
                    {
                        "get => Entity.Has<{1}>();"
                    },
                    Setters = new List<string>()
                    {
                        "set {{ if(Entity.Has<{1}>() == value) return;",
                        "\tif(value) Entity.Add(new {1}());",
                        "\telse Entity.Remove<{1}>();",
                        "}}",
                    },
                }
            },
            {
                typeof(ITimer).FullName, new ComponentTemplate()
                {
                    Parents = new() { typeof(ITimer).FullName },
                    Fields = new List<string>()
                    {
                        $"public float FullTimeValue;",
                        $"public float TimeLeftValue;",
                        "",
                        $"float {typeof(ITimer).FullName}.FullTime => FullTimeValue;",
                        $"float {typeof(ITimer).FullName}.TimeLeft => TimeLeftValue;",
                    },
                    Getters = new List<string>()
                    {
                        "get => Entity.Has<{1}>() ? Entity.Get<{1}>() : null;",
                    },
                }
            },
            {
                "default", new ComponentTemplate()
                {
                    Fields = new() { "public {0} Value;" },
                    Getters = new List<string>()
                    {
                        "get => Entity.Has<{1}>() ? Entity.Get<{1}>().Value : default;",
                    },
                    Setters = new List<string>()
                    {
                        $"set => {typeof(EcsExtensions).FullName}.GetOrCreate<{{1}}>(Entity).Value = value;"
                    }
                }
            }
        };

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