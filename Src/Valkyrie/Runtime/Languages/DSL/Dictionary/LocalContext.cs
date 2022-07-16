using System.Collections.Generic;
using System.Linq;
using Valkyrie.DSL.Actions;
using Valkyrie.Tools;

namespace Valkyrie.DSL.Dictionary
{
    public class LocalContext
    {
        public Dictionary<string, string> Args = new();
        internal List<IDslAction> Actions;

        public void SetValue(string name, string value)
        {
            Args[name] = value;
        }

        public override string ToString() => $"Vars: {Args.Select(x => $"{x.Key}={x.Value}").Join(",")}";
    }
}