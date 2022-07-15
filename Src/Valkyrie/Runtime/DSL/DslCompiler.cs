using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    class Sentence
    {
        public string Text;
    }

    public class DslCompiler
    {
        private readonly DslDictionary _dslDictionary;

        public IDslDictionary Dictionary => _dslDictionary;

        public DslCompiler()
        {
            _dslDictionary = new DslDictionary();
        }

        public object Build(string source)
        {
            var ast = AstProvider.ProgramConstructor.Parse(source.ToStream());

            var sentences = new List<string>();

            Parse(ast, sentences);

            foreach (var sentence in sentences)
            {
                Debug.LogWarning(sentence);
            }

            return true;
        }

        private void Parse(IAstNode ast, List<string> sentences)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<root>":
                case "<sentence_list>":
                    foreach (var astNode in children)
                        Parse(astNode, sentences);
                    break;
                case "<sentence>":
                {
                    var nodes = ast.UnpackNodes(x => x.Name == "<word>");
                    sentences.AddRange(ConvertToString(nodes));
                    break;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        IEnumerable<string> ConvertToString(List<IAstNode> nodes)
        {
            IEnumerable<string> EnumerateStrings(string str)
            {
                foreach (var tail in ConvertToString(nodes.GetRange(1, nodes.Count - 1)))
                    yield return str + tail;
            }

            if (nodes.Count == 0)
            {
                yield return String.Empty;
                yield break;
            }

            var ast = nodes[0].GetChildren()[0];
            var idNodes = ast.UnpackNodes(x => x.Name == "<id>");
            switch (ast.Name)
            {
                case "<control>":
                {
                    var str = $"<{idNodes.Select(x => x.GetString()).Join(" ")}>";
                    foreach (var p in EnumerateStrings(str)) yield return p;
                    yield break;
                }
                case "<id_list>":
                {
                    foreach (var p in idNodes.SelectMany(astNode => EnumerateStrings(astNode.GetString())))
                        yield return p;
                    yield break;
                }
                default:
                    throw new GrammarCompileException(ast, "Unsupported word type");
            }
        }
    }
}