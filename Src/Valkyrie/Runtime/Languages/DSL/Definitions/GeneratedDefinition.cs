using System;
using System.Collections.Generic;
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
}