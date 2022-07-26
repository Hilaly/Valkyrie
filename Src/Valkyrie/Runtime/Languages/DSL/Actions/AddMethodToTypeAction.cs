using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class AddMethodToTypeAction : IDslAction
    {
        public IStringProvider Type;
        public IStringProvider Method;

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var args = lc.GetArgs();
            var type = context.GetOrCreateType(Type.GetString(args));
            var methodName = Method.GetString(args);
            type.GetOrCreateMethod(methodName);
        }

        public override string ToString() => $"{Type} has method {Method}";
    }
}