using System.Collections.Generic;
using UnityEngine;
using Valkyrie.DSL.Definitions;
using Valkyrie.DSL.Dictionary;

namespace Valkyrie.DSL.Actions
{
    class SkipAction : IDslAction
    {
        public override string ToString() => "skip";
        public void Execute(LocalContext args, CompilerContext context) => Debug.LogWarning($"skip");
    }
}