using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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

            public ParseSwitcher AddRecursionAllChildren(string nodeName)
            {
                void Call(Context context, List<IAstNode> children)
                {
                    foreach (var ast in children)
                        Process(context, ast);
                }

                return AddBranch(nodeName, Call);
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
            public MethodImpl Method;
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
            //Debug.Log(text);

            var context = new Context { World = world };
            var ast = AstConstructor.Parse(text.ToStream());
            
            //Debug.Log(ast);
            Parse(context, ast);
        }

        private static void Log(string msg) => Debug.Log($"[GEN]: {msg}");
        private static void LogWarn(string msg) => Debug.LogWarning($"[GEN]: {msg}");


        private static readonly ParseSwitcher WindowElementSwitcher;
        private static readonly ParseSwitcher RootSwitcher;
        private static readonly ParseSwitcher SentenceSwitcher;
        private static readonly ParseSwitcher MethodParseSwitcher;
        private static readonly ParseSwitcher CountersParseSwitcher;

        static WorldModelCompiler()
        {
            RootSwitcher = new ParseSwitcher("RootSwitch")
                .AddRecursionAllChildren("<root>")
                .AddBranch("<full_sentence>", (context, children) =>
                {
                    foreach (var astNode in children.Where(x => x.Name == "<sentence>"))
                        Parse(context, astNode);
                })
                .AddBranch("<sentence>", (context, children) => ParseSentence(context, children[0]));
            
            CountersParseSwitcher = new ParseSwitcher(nameof(ParseCounters))
                .AddBranch("<counter_name>", (context, ast) =>
                {
                    var strName = ast.GetString();
                    if (context.World.Profile.Counters.Contains(strName))
                        LogWarn($"Counter {strName} already defined");
                    context.World.Profile.AddCounter(strName);
                    Log($"Add counter {strName}");
                })
                .AddBranch("<counters_names_list>", (context, children) =>
                {
                    foreach (var c in children.Where(x => x.Name is "<counter_name>" or "<counters_names_list>"))
                        ParseCounters(context, c);
                })
                .AddBranch("<define_counters>", (context, children) =>
                {
                    ParseCounters(context, children.Find(x => x.Name == "<counters_names_list>"));

                });
            
            WindowElementSwitcher = new ParseSwitcher(nameof(ParseWindowElement))
                .AddBranch("<button_define>", ParseButton)
                .AddBranch("<info_define>", ParseInfo);

            SentenceSwitcher = new ParseSwitcher(nameof(ParseSentence))
                .AddBranch("<define_namespace>", ParseNamespace)
                .AddBranch("<define_counters>", ParseCounters)
                .AddBranch("<window_define>", ParseWindow)
                .AddBranch("<event_handler_define>", ParseEventHandler);

            MethodParseSwitcher = new ParseSwitcher(nameof(ParseOp))
                .AddBranch("<op_list>", (context, children) =>
                {
                    foreach (var astNode in children.Where(x => x.Name is "<op_list>" or "<op>"))
                        ParseOp(context, astNode);
                })
                .AddRecursionAllChildren("<op>")
                .AddBranch("<log_op>", ParseLogOp)
                .AddBranch("<cmd_op>", ParseCmdOp)
                .AddBranch("<show_window_op>", ParseWindowOp)
                .AddBranch("<assign_op>", ParseAssignOp);
        }

        #region Switchers Calls

        private static void Parse(Context context, IAstNode ast) => RootSwitcher.Process(context, ast);
        static void ParseWindowElement(Context context, IAstNode ast) => WindowElementSwitcher.Process(context, ast);
        private static void ParseSentence(Context context, IAstNode ast) => SentenceSwitcher.Process(context, ast);
        private static void ParseOp(Context context, IAstNode ast) => MethodParseSwitcher.Process(context, ast);
        private static void ParseCounters(Context context, IAstNode ast) => CountersParseSwitcher.Process(context, ast);


        #endregion
        
        #region Old Switch

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

        #endregion
        
        #region Concrete node parsing

        private static void ParseAssignOp(Context context, List<IAstNode> children)
        {
            var targetNode = children.Find(x => x.Name == "<assign_target>");
            var exprNode = children.Find(x => x.Name == "<expr>");
            var str = ConvertCmdArg(targetNode, context);
            var expr = WriteExpr(context, exprNode);
            context.Method.CodeOp($"{str} = {expr};");
        }

        private static string WriteExpr(Context c, IAstNode ast)
        {
            string r = default;
            var switcher = new ParseSwitcher(nameof(WriteExpr))
                .AddRecursionAllChildren("<expr>")
                .AddBranch("<comp_expr>", (context, children) =>
                {
                    r = WriteExpr(context, children[0]);
                    if (children.Count > 2)
                    {
                        var right = WriteExpr(context, children[2]);
                        r += $" {children[1].GetString()} {right}";
                    }
                })
                .AddBranch("<add_expr>", (context, children) =>
                {
                    r = WriteExpr(context, children[0]);
                    if (children.Count > 2)
                    {
                        var right = WriteExpr(context, children[2]);
                        r += $" {children[1].GetString()} {right}";
                    }
                })
                .AddBranch("<mul_expr>", (context, children) =>
                {
                    r = WriteExpr(context, children[0]);
                    if (children.Count > 2)
                    {
                        var right = WriteExpr(context, children[2]);
                        r += $" {children[1].GetString()} {right}";
                    }
                })
                .AddBranch("<single_expr>", (context, children) =>
                {
                    if (children.Count == 1)
                        r = WriteExpr(context, children[0]);
                    else
                        r = WriteExpr(context, children[1]);
                })
                .AddBranch("<var_expr>", (context, children) =>
                {
                    r = ConvertCmdArg(children[0], context);
                })
                .AddBranch("<const_expr>", (context, children) =>
                {
                    r = children[0].GetString();
                });
            switcher.Process(c, ast);
            return r;
        }

        private static void ParseWindowOp(Context context, List<IAstNode> children)
        {
            var windowName = children.Find(x => x.Name == "<window_name>").GetString();
            context.Method.CommandOp("\"ShowWindow\"", $"\"{windowName}Window\"");
        }
        
        private static void ParseCmdOp(Context context, List<IAstNode> children)
        {
            var cmdName = $"\"{children.Find(x => x.Name == "<cmd_name>").GetString()}\"";
            var args = children.FindAll(x => x.Name == "<arg>").ConvertAll(argNode => ConvertCmdArg(argNode, context));
            context.Method.CommandOp(cmdName, args.ToArray());
        }

        private static string ConvertCmdArg(IAstNode ast, Context context)
        {
            var str = ast.GetString();
            if (context.World.Profile.Counters.Contains(str))
                return $"_profile.{str}";
            
            LogWarn($"Can not determine cmd arg {str}");
            return $"\"{str}\"";
        }

        private static void ParseLogOp(Context context, List<IAstNode> children)
        {
            var msg = children.Find(x => x.Name == "<text_expr>").GetString().Trim('"');
            context.Method.LogOp(msg);
        }

        static void ParseInfo(Context context, List<IAstNode> children)
        {
            var typeNode = children.Find(x => x.Name == "<type_name>");
            var infoName = children.Find(x => x.Name == "<property_name>").GetString();
            Log($"Define info {infoName}");
            var exprNode = children.Find(x => x.Name == "<expr>");
            var expr = WriteExpr(context, exprNode);

            context.Window.AddInfo(GetTypeName(context, typeNode), infoName, expr);
        }

        static string GetTypeName(Context context, IAstNode typeNode)
        {
            var str = typeNode.GetString();
            if (str == "counter")
                return typeof(BigInteger).FullName;
            return str;
        }
        
        static void ParseButton(Context context, List<IAstNode> children)
        {
            var buttonName = children.Find(x => x.Name == "<button_name>").GetString();
            var eventName = context.Window.GetButtonEvent(buttonName);
            Log($"Define event {eventName}");
            var eventEntity = context.World.CreateEvent(eventName);
            
            Log($"Building button {buttonName} at window {context.Window.Name}");
            context.Method = context.Window.DefineButton(buttonName, eventEntity);
            var methodBody = children.Find(x => x.Name == "<method_body>");
            if (methodBody != null)
                MethodParseSwitcher.Process(context, methodBody.GetChildren()[0]);
            context.Method = default;
        }

        private static void ParseEventHandler(Context context, List<IAstNode> children)
        {
            var eventNode = children.Find(x => x.Name == "<event_id>");
            var eventName = ParseEventName(context, eventNode);
            var eventEntity = context.World.Events.Find(x => x.Name == eventName);
            if (eventEntity == null)
            {
                LogWarn($"Event {eventName} not defined, it can be error in source");
                Log($"Define event {eventName}");
                eventEntity = context.World.CreateEvent(eventName);
            }

            Log($"Building handler for {eventName}");
            context.Method = context.World.CreateEventHandler(eventEntity);
            var methodBody = children.Find(x => x.Name == "<method_body>");
            if (methodBody != null)
                MethodParseSwitcher.Process(context, methodBody.GetChildren()[0]);
            context.Method = default;
        }

        private static string ParseEventName(Context c, IAstNode eventNode)
        {
            string id = default;
            var switcher = new ParseSwitcher(nameof(ParseEventName))
                .AddBranch("<button_click_event>", (context, children) =>
                {
                    var buttonName = children.Find(x => x.Name == "<button_name>").GetString();
                    var windowName = children.Find(x => x.Name == "<window_name>").GetString();
                    var window = context.World.Windows.Find(x => x.Name == windowName);
                    id = window.GetButtonEvent(buttonName);
                });
            switcher.Process(c, eventNode.GetChildren()[0]);
            return id;
        }

        private static void ParseNamespace(Context context, List<IAstNode> children)
        {
            var namespaceName = children.Find(x => x.Name == "<namespace_name>").GetString();
            if (context.World.Namespace != nameof(WorldModelInfo) && context.World.Namespace != namespaceName)
                LogWarn($"Changing namespace is bad practice, please use same namespace everywhere.");
            context.World.Namespace = namespaceName;
            Log($"Change namespace to {namespaceName}");
        }
        
        #endregion
    }
}