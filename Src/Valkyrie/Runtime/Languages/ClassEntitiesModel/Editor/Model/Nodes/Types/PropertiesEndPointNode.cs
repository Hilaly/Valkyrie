using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class PropertiesEndPointNode : CemNode, IPersistentNode
    {
        public class Factory : SimpleGenericFactory<PropertiesEndPointNode>
        {
            public Factory() : base($"Properties", "Parts")
            {
            }
        }

        
        [Input("Input", Capacity = Port.Capacity.Multi)]
        public PropertyDefine Input { get; set; }
    }
}