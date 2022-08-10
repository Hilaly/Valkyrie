using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Definitions
{
    [Serializable]
    public class GeneratedTypeDefinition : GeneratedDefinition
    {
        public string TypeCategory { get; set; } = "class";
        public List<string> BaseTypes { get; } = new();
        public List<GeneratedMethodDefinition> Methods { get; } = new();
        public List<GeneratedFieldDefinition> Fields { get; } = new();
        public List<GeneratedPropertyDefinition> Properties { get; } = new();

        public override void Write(FormatWriter sb)
        {
            var classDef = Name;
            if (BaseTypes.Any())
                classDef += " : " + BaseTypes.Join(", ");
            WriteAttributes(sb);
            sb.BeginBlock($"{Modificator} {TypeCategory} {classDef}");

            if(Fields.Any())
            {
                sb.AppendLine("#region Fields");
                sb.AppendLine();
                foreach (var fieldDefinition in Fields) 
                    fieldDefinition.Write(sb);
                sb.AppendLine();
                sb.AppendLine("#endregion //Fields");
            }
            sb.AppendLine();
            if (Properties.Any())
            {
                sb.AppendLine("#region Properties");
                sb.AppendLine();
                foreach (var propertyDefinition in Properties) 
                    propertyDefinition.Write(sb);
                sb.AppendLine();
                sb.AppendLine("#endregion //Properties");
            }
            sb.AppendLine();
            if (Methods.Any())
            {
                sb.AppendLine("#region Methods");
                sb.AppendLine();
                foreach (var methodDefinition in Methods) 
                    methodDefinition.Write(sb);
                sb.AppendLine();
                sb.AppendLine("#endregion //Methods");
            }
            sb.EndBlock();
        }

        public void AddBase(string baseType)
        {
            if(!BaseTypes.Contains(baseType))
                BaseTypes.Add(baseType);
        }

        public GeneratedMethodDefinition GetOrCreateMethod(string name)
        {
            var result = Methods.Find(x => x.Name == name);
            if (result == null)
                Methods.Add(result = new GeneratedMethodDefinition() { Name = name });
            return result;
        }

        public GeneratedFieldDefinition GetOrCreateField(string name)
        {
            var result = Fields.Find(x => x.Name == name);
            if (result == null)
                Fields.Add(result = new GeneratedFieldDefinition() { Name = name });
            return result;
        }

        public GeneratedPropertyDefinition GetOrCreateProperty(string name)
        {
            var result = Properties.Find(x => x.Name == name);
            if (result == null)
                Properties.Add(result = new GeneratedPropertyDefinition() { Name = name });
            return result;
        }
    }
}