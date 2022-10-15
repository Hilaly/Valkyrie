using System.Collections.Generic;
using Valkyrie.Grammar;
using Valkyrie.Language.Ecs;
using Valkyrie.Language.Language.Expressions.Facts;

namespace Valkyrie.Language.Language.Compiler
{
    static class FactsCompiler
    {
        internal static GenerateFactRuntimeMethod CreateFactMethod(string preSentence, List<IAstNode> children,
            LocalVarsDesc localVarDesc, IWorld world)
        {
            var factName = children[0].GetString();
            var factId = world.GetFactId(factName);
            var args = LanguageCompiler.UnpackNodes(children[1], x => x.Name == "<fact_arg>");
            var sentence = preSentence + factName + " " + string.Join(" ", args.ConvertAll(LanguageCompiler.GetText));
            var startupMethod = CompileFact(factId, args, sentence, localVarDesc, world);
            return startupMethod;
        }

        internal static GenerateFactRuntimeMethod CompileFact(int factId, List<IAstNode> args, string sentence,
            LocalVarsDesc localVarDesc, IWorld world)
        {
            var expressions = args.ConvertAll(ast => CompileFactArg(ast, localVarDesc, world));
            return new GenerateFactRuntimeMethod(factId, expressions, sentence);
        }

        internal static IRuntimeExpression CompileFactArg(IAstNode ast, LocalVarsDesc localVarDesc, IWorld world)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<fact_arg>":
                case "<expr>":
                    return CompileFactArg(children[0], localVarDesc, world);
                case "<comp_expr>":
                case "<add_expr>":
                case "<mul_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            var first = CompileFactArg(children[0], localVarDesc, world);
                            var second = CompileFactArg(children[2], localVarDesc, world);
                            switch (children[1].GetString())
                            {
                                case "*":
                                    return new MulRuntimeExpression(first, second);
                                case "/":
                                    return new DelRuntimeExpression(first, second);
                                case "+":
                                    return new AddRuntimeExpression(first, second);
                                case "-":
                                    return new RemRuntimeExpression(first, second);
                                default:
                                    throw new GrammarCompileException(ast,
                                        $"Unsupported operator: {children[1].GetString()}");
                            }
                        case 1:
                            return CompileFactArg(children[0], localVarDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<single_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            return CompileFactArg(children[1], localVarDesc, world);
                        case 1:
                            return CompileFactArg(children[0], localVarDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<const_expr>":
                {
                    switch (children.Count)
                    {
                        case 2:
                            var strValue = children[0].GetString() + children[1].GetString();
                            return new ConstRuntimeExpression(new Variable(float.Parse(strValue)));
                        case 1:
                            return CompileFactArg(children[0], localVarDesc, world);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<var_expr>":
                {
                    var varName = ast.GetString();
                    if (localVarDesc.HasString(varName))
                        return new ReadLocalVariableRuntimeExpression(localVarDesc.GetId(varName));
                    else
                        return new ReadLocalOrGetNewVariableRuntimeExpression(localVarDesc.GetId(varName));
                }
                case "NUMBER":
                    return new ConstRuntimeExpression(new Variable(ast.GetFloat()));
                case "STRING":
                    return new ConstRuntimeExpression(new Variable(string.Intern(ast.GetString().Trim('"'))));
                case "BOOL":
                    return new ConstRuntimeExpression(new Variable(ast.GetBool()));
                case "NULL":
                    return new ConstRuntimeExpression(Variable.Null);
                default:
                    throw new GrammarCompileException(ast, $"Unimplemented node: {name}");
            }
        }
    }
}