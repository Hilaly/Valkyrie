using Valkyrie.DSL.Actions;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Expressions
{
    class GlobalExpr : IDslExpr
    {
        public IStringProvider Name { get; set; }
        
        public bool Validate(LocalContext localContext, CompilerContext compilerContext)
        {
            var args = compilerContext.GlobalVariables;
            var name = Name.GetString(localContext.GetLocalVariables(), args);
            return args.ContainsKey(name);
        }
    }
}