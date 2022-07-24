using System.Collections.Generic;
using Valkyrie.DSL.Definitions;

namespace Valkyrie.DSL.Actions
{
    interface IDslAction
    {
        void Execute(Dictionary<string, string> args, CompilerContext context);
    }
}