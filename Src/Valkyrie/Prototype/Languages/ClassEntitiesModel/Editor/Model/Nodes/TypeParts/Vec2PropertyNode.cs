using UnityEngine;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class Vec2PropertyNode : GenericPropertyNode<Vector2>
    {
        public class Factory : SimpleGenericFactory<Vec2PropertyNode>
        {
            public Factory() : base("Vec2", "Properties") { }
        }
    }
}