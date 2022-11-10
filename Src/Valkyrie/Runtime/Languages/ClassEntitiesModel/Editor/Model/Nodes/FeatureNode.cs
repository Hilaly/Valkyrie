using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Model.Nodes
{
    class FeatureNode : CemGraph
    {
        public class Factory : SimpleGenericFactory<FeatureNode>
        {
            public Factory() : base("Feature", "Project")
            {
            }
        }
        
        public override IEnumerable<INodeFactory> GetFactories()
        {
            return NodeFactories.GetFeatureLevelNodes();
        }
    }
}