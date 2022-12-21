using UnityEngine;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class Vec3PropertyNode : GenericPropertyNode<Vector3>
    {
        public class Factory : SimpleGenericFactory<Vec3PropertyNode>
        {
            public Factory() : base("Vec3", "Properties") { }
        }
    }
}