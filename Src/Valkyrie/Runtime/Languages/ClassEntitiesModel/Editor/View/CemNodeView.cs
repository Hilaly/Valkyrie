using System;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;
using Valkyrie.Model;

namespace Valkyrie.View
{
    class CemNodeView : UnityEditor.Experimental.GraphView.Node, INodeView, IEditorNodeView
    {
        public IEdgeConnectorListener EdgeListener { get; set; }
        
        public Model.INode Node => (INode)userData;

        bool INodeView.IsMovable => true;

        public VisualElement GetRoot() => mainContainer;

        public void Init(INode node)
        {
            name = node.Uid;
            
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
            
            InitializePorts();
            
            RefreshExpandedState();
        }
        void InitializePorts()
        {
            var listener = EdgeListener;

            foreach (var port in Node.Ports.OfType<IOutputPort>()) AddPort(port);
            foreach (var port in Node.Ports.OfType<IInputPort>()) AddPort(port);
        }

        private void AddPort(IPort port)
        {
            var portView = new CemPortView(port.Orientation, port.Direction, port.Capacity, port.Type, EdgeListener);
            
            portView.AddToClassList($"Port_{port.GetType()}");
            portView.portName = port.Name;
            portView.userData = port;
            portView.name = port.Uid;
            
            switch (port.Direction)
            {
                case Direction.Input:
                    inputContainer.Add(portView);
                    break;
                case Direction.Output:
                    outputContainer.Add(portView);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
        }
        
        
    }
}