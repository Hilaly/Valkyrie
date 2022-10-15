using System;
using System.Collections.Generic;
using Valkyrie.Language.Description.Utils;

namespace Valkyrie.DSL.Definitions
{
    [Serializable]
    public class GeneratedMethodDefinition : GeneratedDefinition
    {
        public List<string> Code = new();
        public string Result { get; set; }

        public override void Write(FormatWriter sb)
        {
            WriteAttributes(sb);
            sb.BeginBlock($"{Modificator} {Result ?? "void"} {Name}()");
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
}