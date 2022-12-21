using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    interface IDslAction
    {
        void Execute(LocalContext localContext, CompilerContext context);
    }
}