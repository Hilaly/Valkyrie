using System.Collections.Generic;
using System.Linq;
using Valkyrie.Tools;

namespace Valkyrie.Ecs.DSL
{
    public class LocalContext
    {
        public Dictionary<string, string> Args = new Dictionary<string, string>();
        
        public void SetValue(string name, string value)
        {
            Args[name] = value;
        }

        public override string ToString() => $"Vars: {Args.Select(x => $"{x.Key}={x.Value}").Join(",")}";
    }
}