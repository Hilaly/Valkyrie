using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Expressions
{
    interface IDslExpr
    {
        bool Validate(LocalContext localContext, CompilerContext compilerContext);
    }
}