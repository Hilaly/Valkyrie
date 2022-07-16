using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DSL.Actions;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    class DslDictionaryEntry : IDslDictionaryEntry
    {
        private Regex _regex;
        
        public List<DslDictionaryFormatEntry> Format { get; set; } = new List<DslDictionaryFormatEntry>();
        public List<IDslAction> Actions { get; set; } = new List<IDslAction>();

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
            {
                foreach (Group matchGroup in match.Groups)
                    localContext.SetValue(matchGroup.Name, matchGroup.Value);
                localContext.Actions = Actions;
            }
            return match.Success;
        }

        private Regex BuildRegex()
        {
            var parts = Format.Select(formatEntry =>
            {
                return formatEntry switch
                {
                    IdentifierFormatEntry idEntry => ($"(?<{idEntry.Text}>[\\w]+)"),
                    OperatorFormatEntry opEntry => (opEntry.Text),
                    _ => string.Empty
                };
            });
            var regExpr = $"^{parts.Join(" ")}$";
            return new Regex(regExpr);
        }
    }
}