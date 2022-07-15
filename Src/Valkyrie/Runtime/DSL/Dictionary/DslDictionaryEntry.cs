using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using UnityEngine;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    class DslDictionaryEntry : IDslDictionaryEntry
    {
        private Regex _regex;
        
        public List<DslDictionaryFormatEntry> Format { get; set; } = new List<DslDictionaryFormatEntry>();
        public List<string> Actions { get; set; } = new List<string>();

        public override string ToString()
        {
            return $"{Format.Join(" ")} => {Actions.Join(", ")}.";
        }

        public bool TryMatch(string text, LocalContext localContext)
        {
            if (_regex == null) 
                _regex = BuildRegex();
            var match = _regex.Match(text);
            if (match.Success)
                foreach (Group matchGroup in match.Groups)
                    localContext.SetValue(matchGroup.Name, matchGroup.Value);
            return match.Success;
        }

        private Regex BuildRegex()
        {
            var sb = new StringBuilder();
            foreach (var formatEntry in Format)
            {
                switch (formatEntry)
                {
                    case IdentifierFormatEntry idEntry:
                    {
                        sb.Append($"(?<{idEntry.Text}>[\\w]+)");
                        break;
                    }
                    case OperatorFormatEntry opEntry:
                    {
                        sb.Append(opEntry.Text);
                        break;
                    }
                }
            }
            sb.Append("$");
            var regExpr = sb.ToString();
            Debug.LogWarning(regExpr);
            return new Regex(regExpr);
        }
    }
}