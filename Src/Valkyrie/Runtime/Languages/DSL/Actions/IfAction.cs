using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.Expressions;

namespace Valkyrie.DSL.Actions
{
    class IfAction : IDslAction
    {
        public IDslExpr Expr { get; set; }
        public IDslAction TrueAction { get; set; }
        public IDslAction FalseAction { get; set; }
        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var action = Expr.Validate(localContext, context)
                ? TrueAction
                : FalseAction;

            action?.Execute(localContext, context);
        }
    }
}