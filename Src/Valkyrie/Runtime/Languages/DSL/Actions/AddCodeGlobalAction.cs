using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddCodeGlobalAction : IDslAction
    {
        public IStringProvider Code;

        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var args = localContext.GetLocalVariables();
            context.AddCode(Code.GetString(args, context.GlobalVariables));
        }
    }
}