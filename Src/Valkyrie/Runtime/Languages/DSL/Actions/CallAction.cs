using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class CallAction : IDslAction
    {
        public IStringProvider ChildName { get; set; }

        public void Execute(LocalContext lc, CompilerContext context)
        {
            var compiler = context.Compiler;
            var args = lc.GetArgs();
            foreach (var localContext in lc.GetChildren(ChildName.GetString(args)))
                compiler.Execute(localContext, context);
        }
    }
}