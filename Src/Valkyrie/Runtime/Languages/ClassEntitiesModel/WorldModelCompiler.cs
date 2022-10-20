using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Grammar;
using Valkyrie.Tools;

namespace Valkyrie
{
    internal class WorldModelCompiler
    {
        private static IAstConstructor _astConstructor;

        class ParseSwitcher
        {
            private readonly Dictionary<string, Action<Context, IAstNode>> _branches = new();
            
            public string Name { get; }

            public ParseSwitcher(string name)
            {
                Name = name;
            }

            public ParseSwitcher AddBranch(string nodeName, Action<Context, IAstNode> parser)
            {
                _branches.Add(nodeName, parser);
                return this;
            }

            public ParseSwitcher AddBranch(string nodeName, Action<Context, List<IAstNode>> parser)
            {
                void Call(Context context, IAstNode ast)
                {
                    var children = ast.UnpackGeneratedLists();
                    parser(context, children);
                }

                _branches.Add(nodeName, Call);
                return this;
            }

            public void Process(Context context, IAstNode ast)
            {
                var name = ast.Name;
                if (_branches.TryGetValue(name, out var parser))
                    parser(context, ast);
                else
                    throw new GrammarCompileException(ast, $"{Name}: Unknown node {name}");
            }
        }
        
        private class Context
        {
            public WorldModelInfo World;
            public WindowModelInfo Window;
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

        private static void Log(string msg) => Debug.Log($"[GEN]: {msg}");
        private static void LogWarn(string msg) => Debug.LogWarning($"[GEN]: {msg}");


        private static readonly ParseSwitcher WindowElementSwitcher;
        private static readonly ParseSwitcher RootSwitcher;

        static WorldModelCompiler()
        {
            RootSwitcher = new ParseSwitcher("RootSwitch")
                .AddBranch("<root>", (context, children) =>
                {
                    foreach (var astNode in children)
                        Parse(context, astNode);
                })
                .AddBranch("<full_sentence>", (context, children) =>
                {
                    foreach (var astNode in children.Where(x => x.Name == "<sentence>"))
                        Parse(context, astNode);
                })
                .AddBranch("<sentence>", (context, children) => ParseSentence(context, children[0]));
            
            WindowElementSwitcher = new ParseSwitcher(nameof(ParseWindowElement))
                .AddBranch("<button_define>", ParseButton)
                .AddBranch("<info_define>", (context1, node) =>
                {
                    LogWarn($"skip info_define {node.Name}");
                });
        }
        
        private static void Parse(Context context, IAstNode ast) => RootSwitcher.Process(context, ast);
        static void ParseWindowElement(Context context, IAstNode ast) => WindowElementSwitcher.Process(context, ast);

        static void ParseButton(Context context, IAstNode node)
        {
            Debug.LogWarning("skip button");
        }

        private static void ParseSentence(Context context, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<define_namespace>":
                {
                    ParseNamespace(context, children);
                    return;
                }
                case "<define_counters>":
                {
                    ParseCounters(context, ast);
                    break;
                }
                case "<window_define>":
                {
                    ParseWindow(context, ast);
                    break;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        private static void ParseWindow(Context context, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<window_define>":
                {
                    var windowName = children.Find(x => x.Name == "<window_name>").GetString();
                    Log($"Parsing window {windowName}");
                    context.Window = context.World.GetWindow(windowName);
                    Log($"Parsing window {windowName}");
                    var windowBody = children.Find(x => x.Name == "<window_body>");
                    if(windowBody != default)
                        ParseWindow(context, windowBody);
                    context.Window = default;
                    break;
                }
                case "<window_body>":
                {
                    foreach (var child in children.Where(x => x.Name is "<window_body>" or "<window_element>"))
                        ParseWindow(context, child);
                    break;
                }
                case "<window_element>":
                {
                    ParseWindowElement(context, children[0]);
                    break;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        private static void ParseCounters(Context context, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.UnpackGeneratedLists();
            switch (name)
            {
                case "<counter_name>":
                {
                    var strName = ast.GetString();
                    if (context.World.Profile.Counters.Contains(strName))
                        LogWarn($"Counter {strName} already defined");
                    context.World.Profile.AddCounter(strName);
                    Log($"Add counter {strName}");
                    return;
                }
                case "<counters_names_list>":
                {
                    foreach (var c in children.Where(x => x.Name is "<counter_name>" or "<counters_names_list>"))
                        ParseCounters(context, c);
                    break;
                }
                case "<define_counters>":
                {
                    ParseCounters(context, children.Find(x => x.Name == "<counters_names_list>"));
                    break;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unknown node {name}");
            }
        }

        private static void ParseNamespace(Context context, List<IAstNode> children)
        {
            var namespaceName = children.Find(x => x.Name == "<namespace_name>").GetString();
            if (context.World.Namespace != nameof(WorldModelInfo) && context.World.Namespace != namespaceName)
                LogWarn($"Changing namespace is bad practice, please use same namespace everywhere.");
            context.World.Namespace = namespaceName;
            Log($"Change namespace to {namespaceName}");
        }
    }
}