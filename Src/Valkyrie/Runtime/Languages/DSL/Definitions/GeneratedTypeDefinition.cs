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
        public string Name { get; set; }
        public List<string> Attributes { get; } = new List<string>();
        
        public abstract void Write(FormatWriter sb);

        protected void WriteAttributes(FormatWriter sb)
        {
            if(Attributes.Count == 0)
                return;
            sb.AppendLine($"[{Attributes.Join(", ")}]");
        }
    }

    [Serializable]
    public class GeneratedPropertyDefinition : GeneratedDefinition
    {
        public GeneratedMethodDefinition Setter;
        public GeneratedMethodDefinition Getter;
        
        public override void Write(FormatWriter sb)
        {
            sb.AppendLine($"public int {Name} {{ get; set; }}");
        }
    }

    [Serializable]
    public class GeneratedMethodDefinition : GeneratedDefinition
    {
        public override void Write(FormatWriter sb)
        {
            sb.BeginBlock($"public void {Name}()");
            sb.EndBlock();
        }
    }
    
    [Serializable]
    public class GeneratedTypeDefinition : GeneratedDefinition
    {
        public string TypeCategory { get; set; }
        public List<string> BaseTypes { get; } = new();
        public List<GeneratedMethodDefinition> Methods { get; } = new();

        public override void Write(FormatWriter sb)
        {
            var classDef = Name;
            if (BaseTypes.Any())
                classDef += " : " + BaseTypes.Join(", ");
            sb.BeginBlock($"public class {classDef}");
            sb.EndBlock();
        }

        public GeneratedMethodDefinition GetOrCreateMethod(string name)
        {
            var method = Methods.Find(x => x.Name == name);
            if (method == null)
                Methods.Add(method = new GeneratedMethodDefinition() { Name = name });
            return method;
        }
    }
}