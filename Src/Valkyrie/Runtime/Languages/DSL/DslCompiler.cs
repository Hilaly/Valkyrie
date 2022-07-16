using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.DSL.Dictionary;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.DSL
{
    public class DslCompiler
    {
        public static bool RequireControlMarkers = true;
        
        private readonly DslDictionary _dslDictionary;

        public IDslDictionary Dictionary => _dslDictionary;

        public DslCompiler()
        {
            _dslDictionary = new DslDictionary();
        }

        public void Build(string source, CompilerContext compilerContext)
        {
            var ast = AstProvider.ProgramConstructor.Parse(source.ToStream());

            var sentences = new List<string>();

            Parse(ast, sentences);

            var contexts = new List<LocalContext>();
            foreach (var sentence in sentences)
            {
                var ctx = new LocalContext();
                if (!TryMatchSentence(sentence, Dictionary, ctx))
                    throw new Exception($"{sentence} doesn't present in dictionary");
                contexts.Add(ctx);
            }

            foreach (var localContext in contexts) 
                Apply(localContext, compilerContext);
        }

        bool TryMatchSentence(string text, IDslDictionary dictionary, LocalContext localContext)
        {
            foreach (var entry in dictionary.GetEntries)
                if (entry.TryMatch(text, localContext))
                    return true;
            return false;
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
                {
                    if (tail.NotNullOrEmpty())
                        yield return $"{str} {tail}";
                    else
                        yield return str;
                }
            }

            if (nodes.Count == 0)
            {
                yield return string.Empty;
                yield break;
            }

            var ast = nodes[0].GetChildren()[0];
            var idNodes = ast.UnpackNodes(x => x.Name == "<id>");
            switch (ast.Name)
            {
                case "<control>":
                {
                    var str = RequireControlMarkers 
                        ? $"<{idNodes.Select(x => x.GetString()).Join(" ")}>"
                        :  $"{idNodes.Select(x => x.GetString()).Join(" ")}";
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
        
        private void Apply(LocalContext localContext, CompilerContext compilerContext)
        {
            foreach(var command in localContext.Actions)
                command.Execute(localContext.Args, compilerContext);
        }
    }
}