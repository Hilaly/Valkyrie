using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    static class ViewCompiler
    {
        public static void CompileView(WorldDescription worldDescription, IAstNode ast)
        {
            var scope = new ViewScope();
            
            var name = ast.Name;
            if (name != "<world_view>")
                throw new GrammarCompileException(ast, "Not rule node");

            RuleCompiler.PrepareDependencies(worldDescription, ast, scope);

            scope.Name = ast.GetChildren()[2].GetString();

            CompileProperties(worldDescription, ast.GetChildren()[4], scope);

            worldDescription.Views.Add(scope);
        }

        private static void CompileProperties(WorldDescription worldDescription, IAstNode ast, ViewScope scope)
        {
            var nodes = ast.UnpackNodes(x => x.Name == "<property>");
            foreach (var node in nodes) 
                scope.Properties.Add(CompileViewProperty(worldDescription, node, scope));
        }

        private static ViewProperty CompileViewProperty(WorldDescription worldDescription, IAstNode node, ViewScope scope)
        {
            var name = node.Name;
            switch (name)
            {
                case "<property>":
                case "<single_expr>":
                    return CompileViewProperty(worldDescription, node.GetChildren()[0], scope);
                case "<var_expr>":
                {
                    var fieldName = node.GetString();
                    var lv = scope.LocalVariables.Get(fieldName);
                    if (lv != null)
                        return new ViewProperty()
                        {
                            Field = new FieldDescription()
                            {
                                Name = fieldName,
                                Type = lv.FieldDescription.Type
                            },
                            Op = lv.Name
                        };
                    throw new GrammarCompileException(node, $"Unknown local variable {fieldName}");
                }
                default:
                    throw new GrammarCompileException(node, $"Unsupported node '{name}'");
            }
        }
    }
}