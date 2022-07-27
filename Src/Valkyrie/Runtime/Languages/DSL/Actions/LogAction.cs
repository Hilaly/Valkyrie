using UnityEngine;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class LogAction : IDslAction
    {
        public void Execute(LocalContext lc, CompilerContext context)
        {
            Debug.Log(Text.GetString(lc.GetLocalVariables(), context.GlobalVariables));
        }

        public IStringProvider Text { get; set; }
    }
}