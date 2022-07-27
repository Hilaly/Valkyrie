using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Valkyrie.DSL.Actions;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Dictionary
{
    class DslDictionaryEntry : IDslDictionaryEntry
    {
        private Regex _regex;

        public List<DslDictionaryFormatEntry> Format { get; set; } = new();
        public List<IDslAction> Actions { get; set; } = new();

        public override string ToString()
        {
            return $"{Format.Join(" ")} => {Actions.Join(", ")}.";
        }

        public bool TryMatch(string text, LocalContext localContext)
        {
            var temp = text
                .Split(" ", StringSplitOptions.RemoveEmptyEntries)
                .Select(Grammar.Grammar.CreateTerminalNode)
                .ToList();
            return TryMatch(temp, localContext);
        }

        public bool TryMatch(List<IAstNode> sentence, LocalContext localContext)
        {
            if (sentence.Count == 0 || Format.Count == 0)
                return false;
            
            var matchIndex = 0;
            var sequenceIndex = 0;

            var index = Take(sentence, localContext, matchIndex, sequenceIndex);
            if (index != sentence.Count) 
                return false;
            
            localContext.Actions = this.Actions;
            return true;

        }

        int Take(List<IAstNode> sentence, LocalContext localContext, int matchIndex, int sequenceIndex)
        {
            var startIndex = sequenceIndex;
            
            var matchNodes = Format;
            var nodes = sentence;

            var iter = 100;
            bool IsFinished() => iter-- < 0 || matchIndex == matchNodes.Count || sequenceIndex == nodes.Count;
            int IsMatched() => matchIndex == matchNodes.Count ? sequenceIndex : startIndex;

            while (true)
            {
                if (IsFinished())
                    return IsMatched();

                var entry = matchNodes[matchIndex];
                var node = nodes[sequenceIndex];

                var isMatched = true;
                switch (entry)
                {
                    case OperatorFormatEntry constValue:
                    {
                        isMatched = node.GetString() == constValue.Text;
                        sequenceIndex++;
                        break;
                    }
                    case IdentifierFormatEntry loadId:
                    {
                        localContext.SetValue(loadId.Text, node.GetString());
                        sequenceIndex++;
                        break;
                    }
                    case ExtractTreeFormatEntry extractTree:
                    {
                        var treeName = extractTree.Text;
                        var dslNode = extractTree.Dictionary.Get(treeName, false);
                        if (dslNode == null)
                        {
                            isMatched = false;
                            break;
                        }

                        var tests = dslNode.GetEntries.Select(x => (DslDictionaryEntry)x);
                        var anyMatch = false;
                        foreach (var test in tests)
                        {
                            var localCtxCopy = new LocalContext(localContext);
                            var newIndex = test.Take(sentence, localCtxCopy, 0, sequenceIndex);
                            if (newIndex > sequenceIndex)
                            {
                                anyMatch = true;
                                sequenceIndex = newIndex;
                                localContext.AddChild(treeName, localCtxCopy);
                                localCtxCopy.Actions = test.Actions;
                                break;
                            }
                        }

                        if (!anyMatch)
                            return startIndex;
                        
                        break;
                    }
                    default:
                        return startIndex;
                }

                if (!isMatched)
                    return startIndex;

                matchIndex++;
            }
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