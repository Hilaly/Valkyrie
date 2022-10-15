using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Expressions
{
    class AndExpr : IDslExpr
    {
        public IDslExpr Left { get; set; }
        public IDslExpr Right { get; set; }
        
        public bool Validate(LocalContext localContext, CompilerContext compilerContext)
        {
            return Left.Validate(localContext, compilerContext) && Right.Validate(localContext, compilerContext);
        }
    }
}