using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class StringPropertyNode : GenericPropertyNode<string>
    {
        public class Factory : SimpleGenericFactory<StringPropertyNode>
        {
            public Factory() : base("string", "Properties") { }
        }
    }
}