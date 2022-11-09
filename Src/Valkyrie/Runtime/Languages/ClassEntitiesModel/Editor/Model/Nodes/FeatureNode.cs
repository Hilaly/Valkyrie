using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Model.Nodes
{
    public class FeatureNode : CemGraph
    {
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return Enumerable.Empty<INodeFactory>();
        }
    }
}