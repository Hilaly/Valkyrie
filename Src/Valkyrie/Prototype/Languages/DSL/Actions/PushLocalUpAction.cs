using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;
using Valkyrie.DSL.StringWorking;

namespace Valkyrie.DSL.Actions
{
    class PushLocalUpAction : IDslAction
    {
        public IStringProvider LocalVarName;

        public void Execute(LocalContext localContext, CompilerContext context)
        {
            var ln = LocalVarName.GetString(localContext.GetLocalVariables(), context.GlobalVariables);
            localContext.PushVariableUp(ln);
        }

        public override string ToString() => $"Push LV[{LocalVarName}] to up context";
    }
}