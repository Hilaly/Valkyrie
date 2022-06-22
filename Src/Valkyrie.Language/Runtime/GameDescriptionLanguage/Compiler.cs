using System;
using System.Collections.Generic;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    public static class Compiler
    {
        public static object TestWorldDescCreation() => AstProvider.Constructor;
        public static object TestWorldLogicCreation() => AstProvider.LogicConstructor;

        public static void CompileWorldLogic(WorldDescription worldDescription, string text)
        {
            var ast = AstProvider.LogicConstructor.Parse(text);
            CompileWorldLogic(worldDescription, ast);
        }

        private static void CompileWorldLogic(WorldDescription worldDescription, IAstNode ast)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<root>":
                {
                    foreach (var child in children) 
                        CompileWorldLogic(worldDescription, child);
                    return;
                }
                case "<full_sentence>":
                {
                    if(children[0].Name == "COMMENT")
                        return;
                    CompileWorldLogic(worldDescription, children[0]);
                    return;
                }
                case "<sentence>":
                case "<rule>":
                {
                    CompileWorldLogic(worldDescription, children[0]);
                    return;
                }
                case "<startup_rule>":
                {
                    CompileWorldLogic(worldDescription, children[1]);
                    return;
                }
                case "<facts>":
                {
                    var scope = new MethodsScope();
                    var facts = UnpackNodes(ast, x => x.Name == "<fact>");
                    var methods = facts.ConvertAll(x =>
                        FactsCompiler.CompileFact(worldDescription, x, scope.LocalVariables));
                    scope.Methods.AddRange(methods);
                    worldDescription.InitMethods.Add(scope);
                    return;
                }
                case "<dependent_rule>":
                {
                    RuleCompiler.CompileRule(worldDescription, ast);
                    return;
                }
                case "<directive>":
                {
                    DirectivesCompiler.CompileSimDirective(worldDescription, ast);
                    return;
                }
                case "<world_query>":
                {
                    return;
                }
                case "<world_view>":
                {
                    ViewCompiler.CompileView(worldDescription, ast);
                    return;
                }
                case "<fact_declaration>":
                {
                    FactsCompiler.CreateDeclaration(worldDescription, ast);
                    return;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unsupported node name {name}");
            }
        }

        internal static List<IAstNode> UnpackNodes(this IAstNode node, Func<IAstNode, bool> filter)
        {
            var r = new List<IAstNode>();
            if (filter(node))
                r.Add(node);
            foreach (var child in node.GetChildren())
                r.AddRange(UnpackNodes(child, filter));
            return r;
        }
    }
}