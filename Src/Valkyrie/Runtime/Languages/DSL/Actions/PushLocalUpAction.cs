using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class PushLocalUpAction : IDslAction
    {
        public IStringProvider LocalVarName;

        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var ln = LocalVarName.GetString(localContext.GetArgs());
            localContext.PushVariableUp(ln);
        }

        public override string ToString() => $"Push LV[{LocalVarName}] to up context";
    }
}