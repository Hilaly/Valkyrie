using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Model.Nodes
{
    public static class NodeFactories
    {
        public static IReadOnlyList<INodeFactory> GetProjectLevelNodes()
        {
            return new INodeFactory[]
            {
                new FeatureNode.Factory()
            };
        }
        
        public static IReadOnlyList<INodeFactory> GetFeatureLevelNodes()
        {
            return GetTypesNodes()
                    .Union(new INodeFactory[0])
                    .ToList();
        }
        
        public static IReadOnlyList<INodeFactory> GetTypesNodes()
        {
            return new INodeFactory[]
            {
                //Types defines
                new ArchetypeNode.Factory(),
                new ConfigNode.Factory(),
                
                //Types references
                new TypeReferenceNode<ArchetypeNode>.Factory(),
                new TypeReferenceNode<ConfigNode>.Factory(),
            };
        }

        public static IReadOnlyList<INodeFactory> GetInTypeDefinesNodes()
        {
            return new INodeFactory[]
            {
                new Vec2PropertyNode.Factory(),
                new Vec3PropertyNode.Factory(),
                new IntPropertyNode.Factory(),
                new StringPropertyNode.Factory(),
                new BoolPropertyNode.Factory(),
                new FloatPropertyNode.Factory(),
                new CustomPropertyNode.Factory(),
                new InfoNode.Factory()
            };
        }

        public static IReadOnlyList<INodeFactory> GetFlowNodes()
        {
            return new INodeFactory[]
            {
                new IfNode.Factory()
            };
        }
    }
}