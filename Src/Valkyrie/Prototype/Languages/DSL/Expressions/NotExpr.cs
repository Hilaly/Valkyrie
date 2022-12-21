using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Expressions
{
    class NotExpr : IDslExpr
    {
        public IDslExpr Expr { get; set; }
        
        public bool Validate(LocalContext localContext, CompilerContext compilerContext)
        {
            return !Expr.Validate(localContext, compilerContext);
        }
    }
}