using System.Collections.Generic;
using Valkyrie.Grammar;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language.Expressions.Rules;

namespace Valkyrie.Language.Language.Compiler
{
    static class RulesCompiler
    {
        internal static IWorldQuery CompileWorldQuery(IWorld world, List<IAstNode> children)
        {
            var localDesc = new LocalVarsDesc();
            var prerequisitesNodes = LanguageCompiler.UnpackNodes(children[0], x => x.Name == "<rule_op>");
            var prerequisites = CompilePrerequisites(prerequisitesNodes, localDesc, world);
            var args = LanguageCompiler.UnpackNodes(children[2], x => x.Name == "<fact_arg>");
            var expressions = args.ConvertAll(ast => FactsCompiler.CompileFactArg(ast, localDesc, world));
            var worldQuery = new WorldQuery(localDesc, prerequisites, expressions, world);
            return worldQuery;
        }
        
        internal static IRule CompileRule(IWorld world, List<IAstNode> children)
        {
            var localDesc = new LocalVarsDesc();
            var prerequisitesNodes = LanguageCompiler.UnpackNodes(children[0], x => x.Name == "<rule_op>");
            var prerequisites = CompilePrerequisites(prerequisitesNodes, localDesc, world);
            
            var factsNodes = LanguageCompiler.UnpackNodes(children[2], x => x.Name == "<fact>");
            var factMethods = factsNodes.ConvertAll(factNode =>
                FactsCompiler.CreateFactMethod("RULE => ", factNode.GetChildren(), localDesc, world));
            var ruleSentence = new DependantRule(factMethods, localDesc, prerequisites);
            return ruleSentence;
        }

        private static List<IFactIdProvider> CompilePrerequisites(List<IAstNode> astNodes,
            LocalVarsDesc localVarsDesc, IWorld world)
        {
            var result = new List<IFactIdProvider>();

            foreach (var rn in astNodes)
            {
                var astNode = rn.GetChildren()[0];
                var name = astNode.Name;
                switch (name)
                {
                    case "<fact_ref>":
                        result.Add(CompileFactRef(astNode, localVarsDesc, world));
                        break;
                    case "<expr>":
                        result.Add(CompileFactExpr(astNode, localVarsDesc, world));
                        break;
                    default:
                        throw new GrammarCompileException(astNode, $"Unsupported Node name {name}");
                }
            }

            return result;
        }

        private static IFactIdProvider CompileFactRef(IAstNode refNode, LocalVarsDesc localVarsDesc, IWorld world)
        {
            var name = refNode.Name;
            var children = refNode.GetChildren();
            switch (name)
            {
                case "<fact_ref>":
                {
                    var factName = children[0].GetString();
                    var factId = world.GetFactId(factName);
                    var factArgs = LanguageCompiler.UnpackNodes(refNode, x => x.Name == "<fact_ref_arg>");
                    var checks = new List<IFactRefArgCode>();
                    for (var index = 0; index < factArgs.Count; index++)
                    {
                        var x = factArgs[index];
                        checks.Add(CompileFactRefArg(x, localVarsDesc, world, index));
                    }

                    return new FactRefProvider(factId, checks);
                }
                default:
                    throw new GrammarCompileException(refNode, $"Unsupported Node name {name}");
            }
        }

        private static IFactRefArgCode CompileFactRefArg(IAstNode astNode, LocalVarsDesc localVarsDesc, IWorld world,
            int argIndex)
        {
            var name = astNode.Name;
            var children = astNode.GetChildren();
            switch (name)
            {
                case "<fact_ref_arg>":
                    return CompileFactRefArg(children[0], localVarsDesc, world, argIndex);
                case "<identifier>":
                {
                    var localIndex = localVarsDesc.GetId(astNode.GetString());
                    return new TryLoadFactArgToLocalVariables(argIndex, localIndex);
                }
                case "<single_expr>":
                {
                    var expression = FactsCompiler.CompileFactArg(astNode, localVarsDesc, world);
                    return new CheckValidArg(argIndex, expression);
                }
                default:
                    throw new GrammarCompileException(astNode, $"Unsupported Node name {name}");
            }
        }

        private static DefaultExprCode CompileFactExpr(IAstNode ast, LocalVarsDesc localVarsDesc, IWorld world)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<comp_expr>":
                case "<add_expr>":
                case "<mul_expr>":
                    switch (children.Count)
                    {
                        case 3:
                        {
                            var left = CompileFactExpr(children[0], localVarsDesc, world);
                            var right = CompileFactExpr(children[2], localVarsDesc, world);
                            var op = children[1].GetString();
                            switch (op)
                            {
                                case "<=":
                                    return new LessOrEqualOperatorCode(left, right);
                                case "<":
                                    return new LessOperatorCode(left, right);
                                case ">=":
                                    return new MoreOrEqualOperatorCode(left, right);
                                case ">":
                                    return new MoreOperatorCode(left, right);
                                case "==":
                                    return new EqualOperatorCode(left, right);
                                case "!=":
                                    return new NotEqualOperatorCode(left, right);
                                case "+":
                                    return new MathOpExprCode(left, right, (l, r) => l + r);
                                case "-":
                                    return new MathOpExprCode(left, right, (l, r) => l - r);
                                case "*":
                                    return new MathOpExprCode(left, right, (l, r) => l * r);
                                case "/":
                                    return new MathOpExprCode(left, right, (l, r) => l / r);
                                default:
                                    throw new GrammarCompileException(ast, $"Unsupported operator: {op}");
                            }
                        }
                        case 1:
                            return CompileFactExpr(children[0], localVarsDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                case "<single_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            return CompileFactExpr(children[1], localVarsDesc, world);
                        case 1:
                            return CompileFactExpr(children[0], localVarsDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<var_expr>":
                {
                    var localIndex = localVarsDesc.GetId(ast.GetString());
                    return new LoadLocalVariableCode(localIndex);
                }
                case "<const_expr>":
                {
                    switch (children.Count)
                    {
                        case 2:
                            var strValue = children[0].GetString() + children[1].GetString();
                            return new ConstCode(new Variable(float.Parse(strValue)));
                        case 1:
                            return CompileFactExpr(children[0], localVarsDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "NUMBER":
                    return new ConstCode(new Variable(ast.GetFloat()));
                case "STRING":
                    return new ConstCode(new Variable(string.Intern(ast.GetString().Trim('"'))));
                case "BOOL":
                    return new ConstCode(new Variable(ast.GetBool()));
                case "NULL":
                    return new ConstCode(Variable.Null);
                case "<expr>":
                    return CompileFactExpr(children[0], localVarsDesc, world);
                default:
                    throw new GrammarCompileException(ast, $"Unsupported Node name {name}");
            }
        }
    }
}