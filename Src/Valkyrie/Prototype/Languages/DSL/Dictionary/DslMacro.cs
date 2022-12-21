using System.Text.RegularExpressions;

namespace Valkyrie.DSL.Dictionary
{
    internal class DslMacro : IDslMacro
    {
        private readonly Regex _regex;
        
        public DslMacro(string pattern, string replacement)
        {
            _regex = new Regex(pattern);
            Replacement = replacement;
        }

        public string Pattern => _regex.ToString();
        public string Replacement { get; }
        public bool IsMatch(string text) => _regex.IsMatch(text);
        public string Apply(string text) => _regex.Replace(text, Replacement);
    }
}