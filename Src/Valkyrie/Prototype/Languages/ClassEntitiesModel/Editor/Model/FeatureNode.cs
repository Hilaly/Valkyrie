using System.Collections.Generic;
using Valkyrie.Model.Nodes;

namespace Valkyrie.Model
{
    class FeatureNode : CemGraph
    {
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return NodeFactories.GetTypesNodes();
        }
    }
}