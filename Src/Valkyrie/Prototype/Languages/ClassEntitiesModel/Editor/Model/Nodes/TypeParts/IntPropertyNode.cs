using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class IntPropertyNode : GenericPropertyNode<int>
    {
        public class Factory : SimpleGenericFactory<IntPropertyNode>
        {
            public Factory() : base("int", "Properties") { }
        }
    }
}