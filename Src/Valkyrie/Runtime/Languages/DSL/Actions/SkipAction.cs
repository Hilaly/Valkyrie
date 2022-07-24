using System.Collections.Generic;
using UnityEngine;
using Valkyrie.DSL.Definitions;

namespace Valkyrie.DSL.Actions
{
    class SkipAction : IDslAction
    {
        public override string ToString() => "skip";
        public void Execute(Dictionary<string, string> args, CompilerContext context) => Debug.LogWarning($"skip");
    }
}