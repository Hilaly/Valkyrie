using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class FloatPropertyNode : GenericPropertyNode<float>
    {
        public class Factory : SimpleGenericFactory<FloatPropertyNode>
        {
            public Factory() : base("float", "Properties") { }
        }
    }
}