using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ConfigNode : CemNode
    {
        public class Factory : SimpleGenericFactory<ConfigNode>
        {
            public Factory() : base("Config", "Types")
            {
            }
        }

        public override void OnCreate()
        {
            CreateInputPort<ConfigNode>("Parents").Capacity = Port.Capacity.Multi;
            CreateOutputPort<ConfigNode>("Children").Capacity = Port.Capacity.Multi;
        }
    }
}