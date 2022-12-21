using System;
using System.Collections.Generic;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Language.Compiler
{
    public static class LanguageCompiler
    {
        internal static void Compile(IWorld world, IAstNode ast, string preSentence)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "SENTENCE_OP":
                    return;
                case "<full_sentence>":
                {
                    if (children.Count == 1)
                        Compile(world, children[0], preSentence);
                    return;
                }
                case "<root>":
                case "<sentence>":
                case "<rule>":
                    foreach (var child in children)
                        Compile(world, child, preSentence);
                    return;
                case "<dependent_rule>":
                {
                    var ruleSentence = RulesCompiler.CompileRule(world, children);
                    world.AddRule(ruleSentence);
                    return;
                }
                case "<startup_rule>":
                {
                    Compile(world, children[1], preSentence + children[0].GetString() + " ");
                    return;
                }
                case "<facts>":
                {
                    var localDesc = new LocalVarsDesc();
                    var factsNodes = UnpackNodes(ast, x => x.Name == "<fact>");
                    var factsMethods = factsNodes.ConvertAll(factNode =>
                        FactsCompiler.CreateFactMethod(preSentence, factNode.GetChildren(), localDesc, world));
                    var rule = new StartupScriptRule(factsMethods, localDesc);
                    world.AddStartupRule(rule);
                    return;
                }
                case "<world_query>":
                {
                    var worldQuery = RulesCompiler.CompileWorldQuery(world, children[2].GetChildren());
                    var queryName = children[0].GetString();
                    world.AddWorldQuery(queryName, worldQuery);
                    return;
                }
                case "<world_view>":
                case "<directive>":
                    return;
                default:
                    throw new GrammarCompileException(ast, $"Unimplemented node: {name}");
            }
        }

        internal static List<IAstNode> UnpackNodes(IAstNode node, Func<IAstNode, bool> filter)
        {
            var r = new List<IAstNode>();
            if (filter(node))
                r.Add(node);
            foreach (var child in node.GetChildren())
                r.AddRange(UnpackNodes(child, filter));
            return r;
        }

        internal static string GetText(IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<fact_arg>":
                case "<expr>":
                    return GetText(children[0]);
                case "<comp_expr>":
                case "<add_expr>":
                case "<mul_expr>":
                    switch (children.Count)
                    {
                        case 3:
                            return GetText(children[0]) + children[1].GetString() + GetText(children[2]);
                        case 1:
                            return GetText(children[0]);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                case "<single_expr>":
                    switch (children.Count)
                    {
                        case 3:
                            return children[0].GetString() + GetText(children[1]) + children[2].GetString();
                        case 1:
                            return GetText(children[0]);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                case "<const_expr>":
                    return string.Join(string.Empty, children.ConvertAll(x => x.GetString()));
                case "<var_expr>":
                    return ast.GetString();
                default:
                    throw new GrammarCompileException(ast, $"GetText is not implemented: {name}");
            }
        }
    }
}