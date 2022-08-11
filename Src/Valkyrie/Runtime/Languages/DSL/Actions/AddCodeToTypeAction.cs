using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddCodeToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Code;

        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var args = localContext.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            type.AddCode(Code.GetString(args, context.GlobalVariables));
        }
    }
}