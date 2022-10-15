using System;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.DSL.Definitions
{
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
}