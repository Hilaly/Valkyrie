using System;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.DSL.Definitions
{
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
}