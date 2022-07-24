using System.Collections.Generic;
using UnityEngine;
using Valkyrie.DSL.Definitions;

namespace Valkyrie.DSL.Actions
{
    class LogAction : IDslAction
    {
        public void Execute(Dictionary<string, string> args, CompilerContext context)
        {
            Debug.Log(Text.GetString(args));
        }

        public IStringProvider Text { get; set; }
    }
}