using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Language.Description.Utils;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Definitions
{
    [Serializable]
    public abstract class GeneratedDefinition : IWritable
    {
        public string Modificator { get; set; } = "public";
        public string Name { get; set; }
        public List<string> Attributes { get; } = new();
        
        public abstract void Write(FormatWriter sb);

        protected void WriteAttributes(FormatWriter sb)
        {
            if(Attributes.Count == 0)
                return;
            sb.AppendLine($"[{Attributes.Join(", ")}]");
        }
        
        public void AddAttribute(string attribute)
        {
            if(!Attributes.Contains(attribute))
                Attributes.Add(attribute);
        }
    }

    [Serializable]
    public class GeneratedFieldDefinition : GeneratedDefinition
    {
        public string Type { get; set; }
        
        public override void Write(FormatWriter sb)
        {
            WriteAttributes(sb);
            sb.AppendLine($"{Modificator} {Type} {Name};");
        }
    }

    [Serializable]
    public class GeneratedPropertyDefinition : GeneratedDefinition
    {
        public string Type { get; set; }
        
        public GeneratedMethodDefinition Setter;
        public GeneratedMethodDefinition Getter;
        
        public override void Write(FormatWriter sb)
        {
            WriteAttributes(sb);
            if (Setter == null && Getter == null)
                sb.AppendLine($"{Modificator} {Type} {Name}" + " { get; set; }");
            else
            {
                sb.BeginBlock($"{Modificator} {Type} {Name}");
                if (Getter != null)
                {
                    sb.BeginBlock("get");
                    Getter.WriteCode(sb);
                    sb.EndBlock();
                }
                if (Setter != null)
                {
                    sb.BeginBlock("set");
                    Setter.WriteCode(sb);
                    sb.EndBlock();
                }
                sb.EndBlock();
            }
        }

        public GeneratedMethodDefinition GetGetter()
        {
            return Getter ??= new GeneratedMethodDefinition();
        }

        public GeneratedMethodDefinition GetSetter()
        {
            return Setter ??= new GeneratedMethodDefinition();
        }
    }

    [Serializable]
    public class GeneratedMethodDefinition : GeneratedDefinition
    {
        public List<string> Code = new();

        public override void Write(FormatWriter sb)
        {
            WriteAttributes(sb);
            sb.BeginBlock($"{Modificator} void {Name}()");
            WriteCode(sb);
            sb.EndBlock();
        }

        public void WriteCode(FormatWriter sb)
        {
            foreach (var c in Code) 
                sb.AppendLine(c);
        }

        public void AddCode(string strCode)
        {
            Code.Add(strCode);
        }
    }
    
    [Serializable]
    public class GeneratedTypeDefinition : GeneratedDefinition
    {
        public string TypeCategory { get; set; }
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
            sb.BeginBlock($"{Modificator} class {classDef}");
            sb.AppendLine("#region Fields");
            sb.AppendLine();
            foreach (var fieldDefinition in Fields) 
                fieldDefinition.Write(sb);
            sb.AppendLine();
            sb.AppendLine("#endregion //Fields");
            sb.AppendLine();
            sb.AppendLine("#region Properties");
            sb.AppendLine();
            foreach (var propertyDefinition in Properties) 
                propertyDefinition.Write(sb);
            sb.AppendLine();
            sb.AppendLine("#endregion //Properties");
            sb.AppendLine();
            sb.AppendLine("#region Methods");
            sb.AppendLine();
            foreach (var methodDefinition in Methods) 
                methodDefinition.Write(sb);
            sb.AppendLine();
            sb.AppendLine("#endregion //Methods");
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