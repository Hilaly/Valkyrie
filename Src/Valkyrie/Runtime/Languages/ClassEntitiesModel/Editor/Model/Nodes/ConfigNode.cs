using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ConfigNode : TypeDefineNode<ConfigNode>
    {
        public class Factory : SimpleGenericFactory<ConfigNode>
        {
            public Factory() : base("Config", "Types")
            {
            }
        }

        public override IEnumerable<INodeFactory> GetFactories()
        {
            return Enumerable.Empty<INodeFactory>();
        }
    }
}