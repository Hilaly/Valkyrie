using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    public static partial class WorldExtension
    {
        class ComponentTemplate
        {
            public List<string> Parents = new();
            public List<string> Fields = new();

            public void Write(IComponentInfo info, FormatWriter sb)
            {
                var header = $"struct {info.Name}";
                if (Parents.Any())
                    header += " : " + string.Join(", ", Parents);

                sb.WriteBlock(header, () =>
                {
                    var typeName = info.GetTypeName();
                    foreach (var field in Fields)
                        sb.AppendLine(string.Format(field, typeName));
                });
            }
        }

        private static readonly Dictionary<string, ComponentTemplate> ComponentTemplates = new()
        {
            { typeof(bool).FullName, new ComponentTemplate() },
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
                    }
                }
            },
            {
                "default", new ComponentTemplate()
                {
                    Fields = new() { "public {0} Value;" }
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
            if (!ComponentTemplates.TryGetValue(info.GetTypeName(), out var template))
                template = ComponentTemplates["default"];
            template.Write(info, sb);
        }
    }
}