using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model.Nodes
{
    class IfNode : CemNode
    {
        [Input("Input", Capacity = Port.Capacity.Multi)]
        public IFlow Input { get; set; }
        
        [Input("Condition", Capacity = Port.Capacity.Single)]
        public bool Condition { get; set; }
        
        [Output("True", Capacity = Port.Capacity.Single)]
        public IFlow TrueOutput { get; set; }
        
        [Output("False", Capacity = Port.Capacity.Single)]
        public IFlow FalseOutput { get; set; }
        
        public class Factory : SimpleGenericFactory<IfNode>
        {
            public Factory() : base($"If", "Flow")
            {
            }
        }
    }
}