using System;
using System.Collections.Generic;
using System.Linq;
using Utils;
using Valkyrie.Ecs;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    internal interface IComponentTemplate
    {
        IReadOnlyList<IPropertyInfo> GetRequiredProperties(IComponentInfo info);
        
        void Write(IComponentInfo info, FormatWriter sb);
        void WriteGetter(IPropertyInfo info, FormatWriter sb);
        void WriteSetter(IPropertyInfo info, FormatWriter sb);
        void WriteInit(IPropertyInfo property, FormatWriter sb);
    }

    class EventComponentTemplate : IComponentTemplate
    {
        private readonly NativeTypeEventArchetype _archetype;

        public EventComponentTemplate(NativeTypeEventArchetype archetype)
        {
            _archetype = archetype;
        }

        private string GetComponentFullName(IComponentInfo info) => info.Name.GetComponentFullName();

        public IReadOnlyList<IPropertyInfo> GetRequiredProperties(IComponentInfo info) => _archetype.Properties;

        public void Write(IComponentInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(info);
            var header = $"struct {infoName}";
            //if (Parents.Any()) header += " : " + string.Join(", ", Parents);

            sb.WriteBlock(header, () =>
            {
                foreach (var propertyInfo in _archetype.Properties)
                    sb.AppendLine($"public {propertyInfo.GetTypeName().ToFullName()} {propertyInfo.Name};");
            });
        }

        public void WriteGetter(IPropertyInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(_archetype);
            sb.AppendLine($"get => Entity.Get<{infoName}>().{info.Name};");
        }

        public void WriteSetter(IPropertyInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(_archetype);
            sb.AppendLine($"set => Entity.Get<{infoName}>().{info.Name} = value;");
        }

        public void WriteInit(IPropertyInfo info, FormatWriter sb)
        {
            sb.AppendLine($"{info.Name} = {info.Name.ConvertToUnityPropertyName()},");
        }
    }

    class ComponentTemplate : IComponentTemplate
    {
        public List<Func<IComponentInfo, IPropertyInfo>> RequiredProperties { get; set; } = new();
        public List<string> Parents { get; set; } = new();
        public List<string> Fields { get; set; } = new();
        public List<string> Getters { get; set; } = new();
        public List<string> Setters { get; set; } = new();
        public List<string> Initters { get; set; } = new();

        public IReadOnlyList<IPropertyInfo> GetRequiredProperties(IComponentInfo info) => 
            RequiredProperties.ConvertAll(x => x(info));

        private string GetComponentFullName(IComponentInfo info) => info.Name.GetComponentFullName();
        private string GetComponentFullName(IPropertyInfo info) => info.Name.GetComponentFullName();

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

        public void WriteGetter(IPropertyInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(info);
            var typeName = info.GetTypeName().ToFullName();
            foreach (var str in Getters)
                sb.AppendLine(string.Format(str, typeName, infoName));
            if (!Getters.Any())
                sb.AppendLine("get => throw new NotImplementedException();");
        }

        public void WriteSetter(IPropertyInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(info);
            var typeName = info.GetTypeName().ToFullName();
            foreach (var str in Setters)
                sb.AppendLine(string.Format(str, typeName, infoName));
            if (!Setters.Any())
                sb.AppendLine("throw new NotImplementedException();");
        }

        public void WriteInit(IPropertyInfo info, FormatWriter sb)
        {
            var infoName = GetComponentFullName(info);
            var typeName = info.GetTypeName().ToFullName();
            foreach (var str in Initters)
                sb.AppendLine(string.Format(str, typeName, infoName, info.Name.ConvertToUnityPropertyName()));
        }
    }

    partial class WorldExtension
    {
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
                    RequiredProperties = new () { ValueProperty },
                    Fields = new() { "public {0} Value;" },
                    Getters = new List<string>()
                    {
                        "get => Entity.Has<{1}>() ? Entity.Get<{1}>().Value : default;",
                    },
                    Setters = new List<string>()
                    {
                        $"set => {typeof(EcsExtensions).FullName}.GetOrCreate<{{1}}>(Entity).Value = value;"
                    },
                    Initters = new List<string>()
                    {
                        "Value = {2},"
                    }
                }
            }
        };

        static IPropertyInfo ValueProperty(IComponentInfo componentInfo)
        {
            if (componentInfo is IPropertyInfo propertyInfo)
                return propertyInfo;

            throw new NotImplementedException($"Can not convert {componentInfo.Name} to PropertyInfo");
        }
    }
}