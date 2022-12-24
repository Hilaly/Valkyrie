using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie.DSL
{
    public class DslCompiler
    {
        private readonly DslDictionary _dslDictionary;

        public IDslDictionary Dictionary => _dslDictionary;
        public IAstConstructor ProgramParser => AstProvider.ProgramConstructor;

        public DslCompiler()
        {
            _dslDictionary = new DslDictionary();
        }

        public void Build(string source, CompilerContext compilerContext)
        {
            compilerContext.Compiler = this;
            
            var ast = ProgramParser.Parse(source.ToStream());

            var sentences = new List<List<IAstNode>>();

            Parse(ast, sentences);

            var contexts = new List<LocalContext>();
            foreach (var sentence in sentences)
            {
                var ctx = new LocalContext();
                if (TryMatchSentence(sentence, Dictionary, ctx))
                    contexts.Add(ctx);
                else
                {
                    var text = ConvertToString(sentence);
                    compilerContext.UnparsedSentences.Add(text);
                    Debug.LogWarning($"{text} doesn't present in dictionary");
                }
            }
            
            Debug.Log($"===================");
            Debug.Log($"Start compiling ...");

            foreach (var localContext in contexts)
                Execute(localContext, compilerContext);
            
            Debug.Log($"Compile finished");
            Debug.Log($"===================");
        }

        private void Parse(IAstNode ast, List<List<IAstNode>> sentences)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<root>":
                    foreach (var astNode in children)
                        Parse(astNode, sentences);
                    return;
                case "<sentence>":
                {
                    sentences.Add(
                        children
                            .Where(x => x.Name == "<any>")
                            .Select(x => x.GetChildren()[0])
                            .ToList()
                    );
                    return;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        string ConvertToString(List<IAstNode> nodes)
        {
            var sep = " ";
            return nodes.Select(node => node.ConvertTreeToString(sep)).Join(sep);
        }

        internal void Execute(LocalContext localContext, CompilerContext compilerContext)
        {
            compilerContext.Compiler = this;

            foreach (var command in localContext.Actions)
                command.Execute(localContext, compilerContext);
        }


        private bool TryMatchSentence(List<IAstNode> sentence, IDslDictionary dictionary, LocalContext localContext)
        {
            foreach (var entry in dictionary.GetEntries)
            {
                var lc = new LocalContext();
                if (entry.TryMatch(sentence, lc))
                {
                    localContext.ReplaceFrom(lc);
                    return true;
                }
            }
            return false;
        }

        internal static string ConvertToString(IAstNode node)
        {
            switch (node.Name)
            {
                case "CODE_BLOCK":
                    return node.GetString().Trim('`');
                default:
                    return node.GetString();
            }
        }

        public void PostProcess(CompilerContext ctx)
        {
            foreach (var dslMacro in Dictionary.GetMacros) 
                ctx.Macros.Add(dslMacro);
        }
    }
}