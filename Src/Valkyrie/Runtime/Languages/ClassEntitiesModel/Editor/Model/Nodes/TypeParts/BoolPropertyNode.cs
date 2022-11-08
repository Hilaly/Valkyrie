using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class BoolPropertyNode : GenericPropertyNode<bool>
    {
        public class Factory : SimpleGenericFactory<BoolPropertyNode>
        {
            public Factory() : base("bool", "Properties") { }
        }
    }
}