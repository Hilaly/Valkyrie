using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class InfoEndPoint : CemNode, IPersistentNode
    {
        public class Factory : SimpleGenericFactory<InfoEndPoint>
        {
            public Factory() : base($"Infos", "Parts")
            {
            }
        }
        
        [Input("Input", Capacity = Port.Capacity.Multi)]
        public InfoDefine Input { get; set; }
    }
}