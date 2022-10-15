using System.Collections.Generic;
using System.Text;

namespace Valkyrie.Grammar
{
    class GrammarNodeDefinition
    {
        public string Name;
        
        public List<List<string>> Variants { get; } = new List<List<string>>();

        public override string ToString()
        {
            void LogVariant(List<string> variant, StringBuilder stringBuilder)
            {
                if (variant.Count > 0)
                    stringBuilder.Append(string.Join(" ", variant.ToArray()));
                else
                    stringBuilder.Append('"', 2);
            }

            var sb = new StringBuilder(Name);
            sb.Append(" -> { ");
            
            LogVariant(Variants[0], sb);
            for (var index = 1; index < Variants.Count; index++)
            {
                sb.Append(" | ");
                LogVariant(Variants[index], sb);
            }

            sb.Append(" }");

            return sb.ToString();
        }
    }
}