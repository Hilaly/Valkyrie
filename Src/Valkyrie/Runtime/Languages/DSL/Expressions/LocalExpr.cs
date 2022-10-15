using Valkyrie.DSL.Actions;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Expressions
{
    class LocalExpr : IDslExpr
    {
        public IStringProvider Name { get; set; }
        
        public bool Validate(LocalContext localContext, CompilerContext compilerContext)
        {
            var args = localContext.GetLocalVariables();
            var name = Name.GetString(args, compilerContext.GlobalVariables);
            return args.ContainsKey(name);
        }
    }
}