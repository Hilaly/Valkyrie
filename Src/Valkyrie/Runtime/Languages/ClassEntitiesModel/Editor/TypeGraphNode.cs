using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public interface IGraphView
    {
        IGraph Graph { get; set; }
        
        void Reload();
        void Save();
    }

    public interface INodeView
    {
        INode Node { get; }
        bool IsMoveable { get; }
        
        public VisualElement ValueInPortContainer { get; }
        public VisualElement ValueOutPortContainer { get; }
        public VisualElement FlowInPortContainer { get; }
        public VisualElement FlowOutPortContainer { get; }

        void Initialize(INode node, IReflectionData info);
        Rect GetPosition();
        void SetPosition(Rect position);
    }

    public interface IEditorNodeView : INodeView
    {
        IEdgeConnectorListener EdgeListener { get; set; }
    }
}