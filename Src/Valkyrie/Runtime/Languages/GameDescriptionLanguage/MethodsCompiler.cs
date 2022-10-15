using System.Linq;
using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    public static class MethodsCompiler
    {
        public static void CompileMethod(WorldDescription worldDescription, IAstNode ast)
        {
            var resultAst = ast.UnpackNodes(x => x.Name == "<return_var>").FirstOrDefault();
            var methodName = ast.UnpackNodes(x => x.Name == "<method_name>").First().GetString();
            var args = ast.UnpackNodes(x => x.Name == "<code_arg>");

            var scope = new MethodScope()
            {
                Name = methodName
            };
            foreach (var argNode in args)
            {
                var arg = new LocalVariableDescription(new FieldDescription()
                {
                    Name = argNode.GetChildren()[1].GetString(),
                    Type = argNode.GetChildren()[0].GetString()
                }) { DefineExternal = true };
                scope.LocalVariables.Variables.Add(arg);
                scope.Args.Add(arg.FieldDescription);
            }
            if (resultAst != null)
                scope.Result = resultAst.GetString();

            var facts = ast.UnpackNodes(x => x.Name == "<fact>");
            var methods = facts.ConvertAll(x =>
                FactsCompiler.CompileFact(worldDescription, x, scope.LocalVariables));
            scope.Methods.AddRange(methods);

            worldDescription.DefinedMethods.Add(scope);
        }
    }
}