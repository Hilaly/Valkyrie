using System;
using System.Collections.Generic;
using System.Linq;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    static class RuleCompiler
    {
        internal static void PrepareDependencies(WorldDescription worldDescription, IAstNode ast, DependentScope scope)
        {
            var children = ast.GetChildren();
            var filters = children[0];
            var entities = CollectEntities(worldDescription, filters, scope);
            scope.Filters.AddRange(entities);
        }
        
        public static void CompileRule(WorldDescription worldDescription, IAstNode ast)
        {
            var scope = new MethodsScope();
            scope.LocalVariables.Variables.Add(
                new LocalVariableDescription(new FieldDescription() { Name = "dt", Type = "float" })
                    { DefineExternal = true });

            var name = ast.Name;
            if (name != "<dependent_rule>")
                throw new GrammarCompileException(ast, "Not rule node");
            
            PrepareDependencies(worldDescription, ast, scope);
            
            var children = ast.GetChildren();
            var facts = children[2].UnpackNodes(x => x.Name == "<fact>");
            var methods = facts.ConvertAll(x =>
                FactsCompiler.CompileFact(worldDescription, x, scope.LocalVariables));
            scope.Methods.AddRange(methods);

            worldDescription.SimulationMethods.Add(scope);
        }

        private static List<FactsFilterMethodDescription> CollectEntities(WorldDescription worldDescription, IAstNode ast, DependentScope scope)
        {
            var r = new List<FactsFilterMethodDescription>();
            var nodes = ast.UnpackNodes(x => x.Name == "<rule_op>");
            //var nodes = ast.UnpackNodes(x => x.Name == "<fact_ref>");
            foreach (var node in nodes.Select(x => x.GetChildren()[0]))
            {
                if (node.Name == "<fact_ref>")
                {
                    var args = node.GetChildren()[1].UnpackNodes(x => x.Name == "<fact_ref_arg>");
                    var componentNode = node.GetChildren()[0];
                    var component = worldDescription.GetOrCreateComponent(componentNode.GetString(), args.Count - 1);
                    
                    var idArg = args[0];
                    if (idArg.GetChildren().Count != 1 || idArg.GetChildren()[0].Name != "<identifier>")
                        throw new GrammarCompileException(idArg, "first argument must be variable");

                    bool isReferenceVariable = false;
                    var idName = idArg.GetString();
                    if (!scope.LocalVariables.Has(idName))
                    {
                        scope.LocalVariables.Variables.Add(new LocalVariableDescription(new FieldDescription()
                        {
                            Name = idName,
                            Type = FactsCompiler.IntName
                        })
                        {
                            Name = idName,
                            DefineExternal = true
                        });
                    }
                    else
                    {
                        isReferenceVariable = true;
                        var local = scope.LocalVariables.Get(idName);
                        if (local.FieldDescription.Type == FactsCompiler.AnyName)
                            local.FieldDescription.Type = FactsCompiler.IntName;
                        else if (local.FieldDescription.Type != FactsCompiler.IntName)
                            throw new GrammarCompileException(node, $"{idName} must be of type {FactsCompiler.IntName}");
                    }
                    var filterDesc = r.Find(x => x.Name == idName);
                    if (filterDesc == null) 
                        r.Add(filterDesc = new FactsFilterMethodDescription() { Name = idName, IsReference = isReferenceVariable });
                    if (!filterDesc.Components.Contains(component))
                        filterDesc.Components.Add(component);

                    filterDesc.Operators.AddRange(CompileFactRef(node, scope, worldDescription, filterDesc));

                    for (var i = 1; i < args.Count; ++i)
                    {
                        if(component.Fields[i-1].Type != FactsCompiler.AnyName && !string.IsNullOrEmpty(component.Fields[i-1].Type))
                            continue;
                        var compiledType = FactsCompiler.ComputeFactArgType(args[i], scope.LocalVariables);
                        if(!string.IsNullOrEmpty(compiledType) && compiledType != FactsCompiler.AnyName)
                            component.Fields[i - 1].Type = compiledType;
                    }
                }
                else
                {
                    var filterDesc = r.Last();
                    var strExpr = CompileFactExpr(node, scope, worldDescription, filterDesc);
                    var newOp = $"if(!({strExpr})) continue;";
                    filterDesc.Operators.Add(newOp);
                }
            }

            return r;
        }

        private static List<string> CompileFactRef(IAstNode refNode, DependentScope scope, WorldDescription world, FactsFilterMethodDescription scopeFilter)
        {
            var checks = new List<string>();
            var name = refNode.Name;
            var children = refNode.GetChildren();
            switch (name)
            {
                case "<fact_ref>":
                {
                    var factName = children[0].GetString();
                    var factArgs = refNode.UnpackNodes(x => x.Name == "<fact_ref_arg>");
                    for (var index = 0; index < factArgs.Count; index++)
                    {
                        var x = factArgs[index];
                        checks.AddRange(CompileFactRefArg(x, scope, world, index, factName, scopeFilter));
                    }
                    break;
                }
                default:
                    throw new GrammarCompileException(refNode, $"Unsupported Node name {name}");
            }

            return checks;
        }

        private static List<string> CompileFactRefArg(IAstNode astNode, DependentScope scope, WorldDescription world,
            int argIndex, string factName, FactsFilterMethodDescription scopeFilter)
        {
            var name = astNode.Name;
            var children = astNode.GetChildren();
            var factDesc = world.Components.Find(x => x.Name == factName);
            if (factDesc == null)
                throw new Exception($"Unknown component type {factName}");
            if (argIndex == 0)
                return new List<string>() { $"//entityId: {scopeFilter.Name}" };
            var fieldDesc = factDesc.Fields[argIndex - 1];
            var fieldValue = $"State.Get<{factDesc.GetTypeName()}>({scopeFilter.Name}).{fieldDesc.Name}";
            switch (name)
            {
                case "<fact_ref_arg>":
                    return CompileFactRefArg(children[0], scope, world, argIndex, factName, scopeFilter);
                case "<identifier>":
                {
                    var localVarName = astNode.GetString();
                    if (scope.LocalVariables.Has(localVarName))
                        return new List<string> { $"if({fieldValue} != {localVarName}) continue;" };
                    scope.LocalVariables.Variables.Add(new LocalVariableDescription(fieldDesc)
                        { Name = localVarName });
                    return new List<string> { $"{localVarName} = {fieldValue};" };
                }
                case "<single_expr>":
                {
                    var strExpr = CompileFactExpr(astNode, scope, world, scopeFilter);
                    return new List<string>() { $"if({fieldValue} != ({strExpr})) continue;" };
                }
                default:
                    throw new GrammarCompileException(astNode, $"Unsupported Node name {name}");
            }
        }

        private static string CompileFactExpr(IAstNode astNode, DependentScope scope, WorldDescription world,
            FactsFilterMethodDescription scopeFilter)
        {
            return FactsCompiler.CompileFactArgCode(world, null, astNode, scope.LocalVariables);
        }
    }
}