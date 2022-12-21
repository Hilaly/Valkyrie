using Valkyrie.Grammar;

namespace Valkyrie.Language.Description
{
    static class DirectivesCompiler
    {
        public static void CompileSimDirective(WorldDescription worldDescription, IAstNode ast)
        {
            worldDescription.SimulationMethods.Add(new SimulateDirective()
            {
                Name = ast.GetChildren()[2].GetString()
            });
        }
    }
}