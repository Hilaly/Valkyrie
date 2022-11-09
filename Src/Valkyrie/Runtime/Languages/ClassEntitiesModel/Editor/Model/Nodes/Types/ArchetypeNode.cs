using System.Collections.Generic;
using System.Linq;
using UnityEngine.Scripting;

namespace Valkyrie.Model.Nodes
{
    [Preserve]
    class ArchetypeNode : TypeDefineNode<ArchetypeNode>
    {
        public class Factory : SimpleGenericFactory<ArchetypeNode>
        {
            public Factory() : base("Archetype", "Types")
            {
            }
        }

        public override IEnumerable<INodeFactory> GetFactories()
        {
            return new INodeFactory[]
            {
                new CustomPropertyNode.Factory()
            };
        }
    }
}