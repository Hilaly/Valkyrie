using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Valkyrie.Grammar
{
    class GrammarDefinition : IGrammarDefinition
    {
        public HashSet<string> Parameters { get; } = new HashSet<string>();
        public List<KeyValuePair<Regex, string>> Lexem { get; } = new List<KeyValuePair<Regex, string>>();
        public List<GrammarNodeDefinition> Nodes { get; } = new List<GrammarNodeDefinition>();

        public Regex EscapeLexem => Lexem.Find(u => u.Value == "escape").Key ?? new Regex("[ \n\r\t]");

        #region Parameters
        
        public bool ReadEscape => Parameters.Contains("escape");
        public bool ReadEol => Parameters.Contains("eol");
        public bool Lexer => Parameters.Contains("lexer");
        public bool Optimise => Parameters.Contains("optimizer");

        #endregion

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine("Grammar:");
            foreach (var param in Parameters) sb.Append("Param: ").AppendLine(param);
            foreach (var lexem in Lexem) sb.AppendLine($"Lexem: {lexem.Key}{lexem.Value ?? ""}");
            foreach (var node in Nodes) sb.Append(" ").AppendLine(node.ToString());
            return sb.ToString();
        }
    }
}