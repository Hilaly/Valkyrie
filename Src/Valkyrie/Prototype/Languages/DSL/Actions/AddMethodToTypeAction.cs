using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class AddMethodToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Method;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetLocalVariables();
            var type = context.GetOrCreateType(Type.GetString(args, context.GlobalVariables));
            var methodName = Method.GetString(args, context.GlobalVariables);
            type.GetOrCreateMethod(methodName);
        }

        public override string ToString() => $"{Type} has method {Method}";
    }
}