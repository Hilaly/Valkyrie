using UnityEditor.Experimental.GraphView;

namespace Valkyrie.GraphDefineImpl
{
    public abstract class TypeDefineNode : BaseNode
    {
        [ValueOut(Name = "Children", Capacity = Port.Capacity.Multi, Orientation = Orientation.Vertical)]
        public TypeDefineNode Children;

        [ValueIn(Name = "Parents", Capacity = Port.Capacity.Multi, Orientation = Orientation.Vertical)]
        public TypeDefineNode Parents;

        protected TypeDefineNode()
        {
            Children = this;
        }
    }
    
    [Node("types", Path = "Types", Name = "Config")]
    public class ConfigDefineNode : TypeDefineNode
    {}
    
    [Node("types", Path = "Types", Name = "Archetype")]
    public class EntityDefineNode : TypeDefineNode
    {}
}