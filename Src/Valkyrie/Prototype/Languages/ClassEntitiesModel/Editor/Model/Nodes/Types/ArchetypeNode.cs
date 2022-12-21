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
    }
}