using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.Composition
{
    internal interface IComponentTemplate
    {
        void Write(IComponentInfo info, FormatWriter sb);
        void WriteGetter(IPropertyInfo info, FormatWriter sb);
        void WriteSetter(IPropertyInfo info, FormatWriter sb);
    }

    class EventComponentTemplate : IComponentTemplate
    {
        private NativeTypeEventArchetype _archetype;

        public EventComponentTemplate(NativeTypeEventArchetype archetype)
        {
            _archetype = archetype;
        }
        
        private string GetComponentFullName(IComponentInfo info) => info.Name.GetComponentFullName();

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
    }

    class ComponentTemplate : IComponentTemplate
    {
        public List<string> Parents { get; set; } = new();
        public List<string> Fields { get; set; } = new();
        public List<string> Getters { get; set; } = new();
        public List<string> Setters { get; set; } = new();

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
    }

}