using System;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    public class NodeViewBase : Node, IEditorNodeView
    {
        public IEdgeConnectorListener EdgeListener { get; set; }
        
        public VisualElement ValueInPortContainer => inputContainer;
        public VisualElement ValueOutPortContainer => outputContainer;
        public VisualElement FlowInPortContainer { get; private set; }
        public VisualElement FlowOutPortContainer { get; private set; }

        public INode Node => (INode)userData;
        public bool IsMoveable => throw new Exception("Not implemented");

        #region API

        protected virtual void OnInitialize()
        {
        }

        protected virtual void OnDestroy()
        {
        }

        protected virtual void OnError()
        {
        }

        #endregion

        public void Initialize(INode node, INodeFactory data)
        {
            userData = node;
            name = node.Uid;
            style.position = Position.Absolute;
            style.left = node.NodePosition.x;
            style.top = node.NodePosition.y;
            style.minWidth = data.MinSize.x;
            style.minHeight = data.MinSize.y;
            title = $"{data.Name}";
            tooltip = data.Tooltip;
            if (!data.Deletable)
            {
                capabilities &= ~Capabilities.Deletable;
            }

            if (!data.Movable)
            {
                capabilities &= ~Capabilities.Movable;
            }

            CreateBody(node);
            CreateFlowPortContainers();
            //CreateExecuteButton(node);
            CreateFlowPorts(node);
            AttachFlowPortContainers();
            CreateValuePorts(node);
            RefreshExpandedState();
            RefreshPorts();

            RegisterCallback<DetachFromPanelEvent>((e) => Destroy());

            OnInitialize();
        }

        private void CreateBody(INode node)
        {
            // TODO: Draw Property Field's with UI Elements
            // http://wiki.unity3d.com/index.php/ExposePropertiesInInspector_Generic
        }

        private void CreateFlowPortContainers()
        {
            FlowInPortContainer = new VisualElement { name = "FlowPorts" };
            FlowInPortContainer.AddToClassList("FlowInPorts");
            FlowOutPortContainer = new VisualElement { name = "FlowPorts" };
            FlowOutPortContainer.AddToClassList("FlowOutPorts");
        }

        private void CreateExecuteButton(INode node)
        {
            /*
            if (!node.IsFlowRoot) return;
            var button = new Button(() => new Flow(Node.Graph, node).Execute()) {text = "Execute"};
            titleButtonContainer.Add(button);
            */
        }

        private void CreateFlowPorts(INode node)
        {
            foreach (var port in node.FlowInPorts)
            {
                FlowInPortContainer.Add(CreatePortView(port, port.Orientation));
            }

            foreach (var port in node.FlowOutPorts)
            {
                FlowOutPortContainer.Add(CreatePortView(port, port.Orientation));
            }
        }

        private CemPortView CreatePortView(IPort port, Orientation orientation)
        {
            var view = new CemPortView(orientation, port.Direction, port.Capacity,
                port.ValueType, EdgeListener)
            {
                name = port.Name,
                userData = port,
                portName = port.Name,
            };
            return view;
        }

        private void AttachFlowPortContainers()
        {
            if (FlowInPortContainer.childCount > 0) mainContainer.parent.Insert(0, FlowInPortContainer);
            if (FlowOutPortContainer.childCount > 0) mainContainer.parent.Add(FlowOutPortContainer);
        }

        private void CreateValuePorts(INode node)
        {
            foreach (var port in node.ValueInPorts)
            {
                ValueInPortContainer.Add(CreatePortView(port, port.Orientation));
            }

            foreach (var port in node.ValueOutPorts)
            {
                ValueOutPortContainer.Add(CreatePortView(port, port.Orientation));
            }
        }

        internal void Destroy()
        {
            OnDestroy();
        }
    }

    public class CemNodeView : NodeViewBase
    {
    }

    class CemPortView : Port
    {
        protected internal CemPortView(Orientation portOrientation, Direction portDirection, Capacity portCapacity,
            Type type, IEdgeConnectorListener listener) : base(portOrientation, portDirection, portCapacity, type)
        {
            m_EdgeConnector = new EdgeConnector<Edge>(listener);
            this.AddManipulator(m_EdgeConnector);
        }
    }
}