using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace Valkyrie.Grammar
{
    class GrammarParser : IAstConstructor
    {
        private readonly GrammarDefinition _grammar;

        public GrammarParser(GrammarDefinition grammar)
        {
            _grammar = grammar;
            if (_grammar.Nodes.Count == 0)
                throw new ArgumentOutOfRangeException($"Grammar definition is empty");
        }

        public ILexer GetLexer()
        {
            return Grammar.CreateLexer(_grammar);
        }

        public IAstNode Parse(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            
            var lexems = GetLexer().Parse(stream).ConvertAll(u => (IAstNode) new TerminalNode(u));

            var targetLexem = _grammar.Nodes[0].Name;

            int startLexemIndex = 0;
            int maxReachIndex = 0;
            List<string> refDescs = null;
            var result = Parse(targetLexem, ref startLexemIndex, lexems, ref maxReachIndex, ref refDescs);
            if (startLexemIndex == lexems.Count)
                return result;
            var matchList = string.Join("[=]", refDescs.ToArray());
            var ss = string.Join("[=]", lexems.GetRange(System.Math.Max(0, maxReachIndex),
                System.Math.Min(10, lexems.Count - maxReachIndex)).Select(u => u.Name).ToArray());
            throw new Exception($"Can not match '{matchList}' near {ss}");
        }

        private IAstNode Parse(string targetLexem, ref int startIndex, List<IAstNode> lexemsCount, ref int maxReachIndex, ref List<string> refDescs)
        {
            var saveIndex = startIndex;

            var definition = _grammar.Nodes.Find(u => u.Name == targetLexem);
            if (definition == null)
                return null;

            for (int variantIndex = 0; variantIndex < definition.Variants.Count; variantIndex++)
            {
                var currentLexemIndex = startIndex;

                var desc = definition.Variants[variantIndex];

                var isMatched = true;

                var nodes = new IAstNode[desc.Count];

                for (var i = 0; i < desc.Count; ++i)
                {
                    if (currentLexemIndex < lexemsCount.Count)
                    {
                        var innerTargetLexem = desc[i];
                        if (IsMatch(innerTargetLexem, lexemsCount[currentLexemIndex]))
                        {
                            nodes[i] = lexemsCount[currentLexemIndex];
                            currentLexemIndex++;
                            continue;
                        }

                        var innerNode = Parse(innerTargetLexem, ref currentLexemIndex, lexemsCount,
                            ref maxReachIndex, ref refDescs);
                        if (innerNode != null && IsMatch(innerTargetLexem, innerNode))
                        {
                            nodes[i] = innerNode;
                            continue;
                        }
                    }

                    isMatched = false;
                    if (currentLexemIndex > maxReachIndex)
                    {
                        maxReachIndex = currentLexemIndex;
                        refDescs = desc;
                    }
                    break;
                }

                if (!isMatched)
                    continue;

                startIndex = currentLexemIndex;
                return new NonTerminalNode(targetLexem, nodes);
            }

            startIndex = saveIndex;
            return null;
        }

        bool IsMatch(string regex, IAstNode node)
        {
            if (node.Name == regex)
                return true;

            try
            {
                var r = new Regex(regex);
                return r.IsMatch(node.Name) || node is TerminalNode terminate && r.IsMatch(terminate.Lexem.Value);
            }
            catch (Exception e)
            {
                throw new Exception($"parsing regex={regex} node={node}", e);
            }
        }
    }
}