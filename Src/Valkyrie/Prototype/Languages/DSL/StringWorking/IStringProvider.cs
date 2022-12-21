using System.Collections.Generic;

namespace Valkyrie.DSL.StringWorking
{
    interface IStringProvider
    {
        string GetString(Dictionary<string, string> localVariables, Dictionary<string, string> globalVariables);
    }
}