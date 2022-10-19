using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie
{
    internal class WorldModelCompiler
    {
        private static IAstConstructor _astConstructor;

        private class Context
        {
            public WorldModelInfo World;
        }

        private static IAstConstructor AstConstructor
        {
            get
            {
                if (_astConstructor == null)
                {
                    var data = Resources.Load<TextAsset>("WorldModelDescriptionGrammar").text;
                    using var dataStream = data.ToStream();
                    _astConstructor = Valkyrie.Grammar.Grammar.Create(dataStream);
                }

                return _astConstructor;
            }
        }
        public static void Parse(WorldModelInfo world, string text)
        {
            Debug.Log(text);

            var context = new Context { World = world };
            var ast = AstConstructor.Parse(text.ToStream());
            
            Debug.Log(ast);
            Parse(context, ast);
        }

        private static void Log(string msg)
        {
            Debug.Log($"[GEN]: {msg}");
        }

        private static void Parse(Context context, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<root>":
                    foreach (var astNode in children)
                        Parse(context, astNode);
                    break;
                case "<full_sentence>":
                    foreach (var astNode in ast.UnpackNodes(x => x.Name == "<sentence>"))
                        Parse(context, astNode);
                    break;
                case "<sentence>":
                    ParseSentence(context, children[0]);
                    break;
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        private static void ParseSentence(Context context, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<define_namespace>":
                {
                    var namespaceName = children.Find(x => x.Name == "<namespace_name>").GetString();
                    context.World.Namespace = namespaceName;
                    Log($"Change namespace to {namespaceName}");
                    return;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }
    }
}