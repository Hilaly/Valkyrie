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
            var args = lc.GetLocalVariables();
            foreach (var localContext in lc.GetChildren(ChildName.GetString(args, context.GlobalVariables)))
                compiler.Execute(localContext, context);
        }
    }
}