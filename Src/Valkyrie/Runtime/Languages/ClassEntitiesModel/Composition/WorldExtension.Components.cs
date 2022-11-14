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

        class ComponentTemplate
        {
            public List<string> Parents = new();
            public List<string> Fields = new();
            public List<string> Getters = new();
            public List<string> Setters = new();

            public string GetComponentFullName(IComponentInfo info) => info.Name.GetComponentFullName();

            public void Write(IComponentInfo info, FormatWriter sb)
            {
                var infoName = GetComponentFullName(info);
                var header = $"struct {infoName}";
                if (Parents.Any())
                    header += " : " + string.Join(", ", Parents);

                sb.WriteBlock(header, () =>
                {
                    var typeName = info.GetTypeName().ToFullName();
                    foreach (var field in Fields)
                        sb.AppendLine(string.Format(field, typeName, infoName));
                });
            }

            public void WriteGetter(IComponentInfo info, FormatWriter sb)
            {
                var infoName = GetComponentFullName(info);
                var typeName = info.GetTypeName().ToFullName();
                foreach (var str in Getters)
                    sb.AppendLine(string.Format(str, typeName, infoName));
                if (!Getters.Any())
                    sb.AppendLine("get => throw new NotImplementedException();");
            }

            public void WriteSetter(IComponentInfo info, FormatWriter sb)
            {
                var infoName = GetComponentFullName(info);
                var typeName = info.GetTypeName().ToFullName();
                foreach (var str in Setters)
                    sb.AppendLine(string.Format(str, typeName, infoName));
                if (!Setters.Any())
                    sb.AppendLine("throw new NotImplementedException();");
            }
        }

        private static readonly Dictionary<string, ComponentTemplate> ComponentTemplates = new()
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

        private static ComponentTemplate GetComponentTemplate(IComponentInfo info)
        {
            if (!ComponentTemplates.TryGetValue(info.GetTypeName(), out var template))
                template = ComponentTemplates["default"];
            return template;
        }
    }
}