using System.Collections.Generic;

namespace Valkyrie.Model.Nodes
{
    public static class NodeFactories
    {
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