using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class InfoNode : CemNode
    {
        public class Factory : SimpleGenericFactory<InfoEndPoint>
        {
            public Factory() : base($"Info define", "Properties")
            {
            }
        }
        
        [Input("Input", Capacity = Port.Capacity.Multi)]
        public IFlow Input { get; set; }

        [ExportProperty(Name = "Info Name")]
        public string InfoName
        {
            get => Output.Name;
            set => Output.Name = value;
        }

        [Output("Output", Capacity = Port.Capacity.Single)]
        public InfoDefine Output { get; set; } = new InfoDefine();
    }
}