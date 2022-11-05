using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Valkyrie.View
{
    class CemNodeView : UnityEditor.Experimental.GraphView.Node, INodeView, IEditorNodeView
    {
        public IEdgeConnectorListener EdgeListener { get; set; }
        
        public Model.INode Node => (Model.INode)userData;

        bool INodeView.IsMovable => true;

        public CemNodeView(Model.INode node)
        {
            userData = node;
            viewDataKey = node.Uid;

            style.position = Position.Absolute;
            style.left = node.NodePosition.x;
            style.top = node.NodePosition.x;
            
            /*
            style.minWidth = node.NodeRect.size.x;
            style.minHeight = node.NodeRect.size.y;
            */
            
            title = node.Name;
        }
    }
}