using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class SetMethodTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Method;
        public IStringProvider Code;
        
        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var args = localContext.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            var prop = type.GetOrCreateMethod(Method.GetString(args, context.GlobalVariables));
            prop.Result = Code.GetString(args, context.GlobalVariables);
        }
    }
}