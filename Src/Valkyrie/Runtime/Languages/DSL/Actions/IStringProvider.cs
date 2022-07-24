using System.Collections.Generic;

namespace Valkyrie.DSL.Actions
{
    interface IStringProvider
    {
        string GetString(Dictionary<string, string> args);
    }
}