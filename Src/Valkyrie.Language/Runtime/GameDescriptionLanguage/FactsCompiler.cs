using System.Collections.Generic;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    public class LocalVariableDescription
    {
        public FieldDescription FieldDescription;
        public bool DefineExternal;
        private string _name;

        public LocalVariableDescription(FieldDescription fieldDescription)
        {
            FieldDescription = fieldDescription;
        }

        public string Name
        {
            get => _name ?? FieldDescription.Name;
            set => _name = value;
        }
    }
    
    public class LocalVariables
    {
        public List<LocalVariableDescription> Variables = new List<LocalVariableDescription>();

        public bool Has(string varName)
        {
            return Get(varName) != null;
        }

        public LocalVariableDescription Get(string varName)
        {
            return Variables.Find(x => x.Name == varName);
        }
    }
    static class FactsCompiler
    {
        public static FactCreationMethodDescription CompileFact(WorldDescription worldDescription, IAstNode ast, LocalVariables localVariables)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<fact>":
                {
                    var factName = children[0].GetString();
                    var argNodes = ast.UnpackNodes(x => x.Name == "<fact_arg>");
                    var componentDescription = worldDescription.GetOrCreateComponent(factName, argNodes.Count-1);
                    var result = new FactCreationMethodDescription(componentDescription);
                    result.EntityIdExpr = CompileFactId(worldDescription, componentDescription, argNodes[0], localVariables);
                    for (var index = 1; index < argNodes.Count; index++)
                    {
                        var node = argNodes[index];
                        result.Arguments.Add(CompileFactArg(worldDescription, componentDescription, node, index - 1, localVariables));
                    }

                    return result;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unsupported node name");
            }
        }

        private static string CompileFactId(WorldDescription worldDescription,
            ComponentDescription componentDescription, 
            IAstNode ast, 
            LocalVariables localVariables)
        {
            var type = ComputeFactArgType(ast, localVariables);
            if (type != IntName && type != AnyName)
                throw new GrammarCompileException(ast, $"First argument of fact must have type 'int', but have {type}");
            return CompileFactArgCode(worldDescription, componentDescription, ast, localVariables);
        }

        private static string CompileFactArg(WorldDescription worldDescription,
            ComponentDescription componentDescription, IAstNode ast, int argIndex, LocalVariables localVariables)
        {
            var field = componentDescription.Fields[argIndex];
            var compiledType = ComputeFactArgType(ast, localVariables);
            if (string.IsNullOrEmpty(field.Type) || field.Type == AnyName)
                field.Type = compiledType;
            if (field.Type != compiledType)
                throw new GrammarCompileException(ast, "Type of field is mismatch");
            return CompileFactArgCode(worldDescription, componentDescription, ast, localVariables);
        }

        internal static string ComputeFactArgType(IAstNode ast, LocalVariables localVariables)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<fact_ref_arg>":
                case "<fact_arg>":
                case "<expr>":
                    return ComputeFactArgType(children[0], localVariables);
                case "<comp_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            return BoolName;
                        case 1:
                            return ComputeFactArgType(children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<mul_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                        {
                            var l = ComputeFactArgType(children[0], localVariables);
                            switch (l)
                            {
                                case StringName:
                                case BoolName:
                                    throw new GrammarCompileException(ast, "Unsupported operation");
                                case FloatName:
                                    return FloatName;
                                default:
                                {
                                    var r = ComputeFactArgType(children[2], localVariables);
                                    switch (r)
                                    {
                                        case IntName:
                                            return IntName;
                                        case FloatName:
                                            return FloatName;
                                        default:
                                            throw new GrammarCompileException(ast, "Unsupported operation");
                                    }
                                }
                            }
                        }
                        case 1:
                            return ComputeFactArgType(children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<add_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                        {
                            var l = ComputeFactArgType(children[0], localVariables);
                            switch (l)
                            {
                                case StringName:
                                    return StringName;
                                case BoolName:
                                    throw new GrammarCompileException(ast, "Unsupported operation");
                                case FloatName:
                                    return FloatName;
                                default:
                                {
                                    var r = ComputeFactArgType(children[2], localVariables);
                                    switch (r)
                                    {
                                        case FloatName:
                                            return FloatName;
                                        case StringName:
                                            throw new GrammarCompileException(ast, "Unsupported operation");
                                        case BoolName:
                                            throw new GrammarCompileException(ast, "Unsupported operation");
                                        case IntName:
                                            return IntName;
                                        default:
                                            throw new GrammarCompileException(ast, "Unsupported operation");
                                    }
                                }
                            }
                        }
                        case 1:
                            return ComputeFactArgType(children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<single_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                            return ComputeFactArgType(children[1], localVariables);
                        case 1:
                            return ComputeFactArgType(children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<const_expr>":
                {
                    switch (children.Count)
                    {
                        case 2:
                            return ComputeFactArgType(children[1], localVariables);
                        case 1:
                            return ComputeFactArgType(children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "NUMBER":
                    return ast.GetString().Contains('.') ? FloatName : IntName;
                case "STRING":
                    return StringName;
                case "BOOL":
                    return BoolName;
                case "NULL":
                    return AnyName;
                case "<var_expr>":
                case "<identifier>":
                {
                    var varName = ast.GetString();
                    if (localVariables.Has(varName))
                        return localVariables.Get(varName).FieldDescription.Type;
                    return AnyName;
                }
                default:
                    throw new GrammarCompileException(ast, $"Unimplemented node: {name}");
            }
        }


        internal static string CompileFactArgCode(WorldDescription worldDescription,
            ComponentDescription componentDescription, IAstNode ast, LocalVariables localVariables)
        {
            var name = ast.Name;
            var children = ast.GetChildren();
            switch (name)
            {
                case "<fact_arg>":
                case "<expr>":
                    return CompileFactArgCode(worldDescription, componentDescription, children[0], localVariables);
                case "<comp_expr>":
                case "<add_expr>":
                case "<mul_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                        {
                            var l = CompileFactArgCode(worldDescription, componentDescription, children[0],
                                localVariables);
                            var r = CompileFactArgCode(worldDescription, componentDescription, children[2],
                                localVariables);
                            var sign = children[1].GetString();
                            return $"{l} {sign} {r}";
                        }
                        case 1:
                            return CompileFactArgCode(worldDescription, componentDescription, children[0],
                                localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "<single_expr>":
                {
                    switch (children.Count)
                    {
                        case 3:
                        {
                            var l = CompileFactArgCode(worldDescription, componentDescription, children[1],
                                localVariables);
                            return $"( {l} )";
                        }
                        case 1:
                            return CompileFactArgCode(worldDescription, componentDescription, children[0],
                                localVariables);
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
                            return strValue;
                        case 1:
                            return CompileFactArgCode(worldDescription, componentDescription, children[0], localVariables);
                        default:
                            throw new GrammarCompileException(ast, $"Unsupported count of children: {children.Count}");
                    }
                }
                case "NUMBER":
                case "STRING":
                case "BOOL":
                    return ast.GetString();
                case "NULL":
                    return "null";
                case "<var_expr>":
                {
                    var strName = ast.GetString();
                    if (localVariables.Has(strName))
                        return strName;
                    else
                    {
                        localVariables.Variables.Add(new LocalVariableDescription(new FieldDescription()
                        {
                            Name = strName,
                            Type = IntName
                        }));
                        return $"({strName} = {GenNewEntityId})";
                    }
                }
                default:
                    throw new GrammarCompileException(ast, $"Unimplemented node: {name}");
            }
        }

        internal const string IntName = "int";
        private const string StringName = "string";
        private const string FloatName = "float";
        private const string BoolName = "bool";
        internal const string AnyName = "any";

        private const string GenNewEntityId = "State.Generate()";
    }
}