using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
using Valkyrie.Model;
using Valkyrie.Window;

namespace Valkyrie.View
{
    public class CemGraphView : GraphView
    {
        public new class UxmlFactory : UxmlFactory<CemGraphView, UxmlTraits>
        {
        }

        private CemSearchProvider _searchProvider;
        private MiniMap _minimap;

        public Model.IGraph Graph { get; internal set; }
        public IEdgeConnectorListener EdgeConnectorListener { get; }

        public CemGraphView()
        {
            EdgeConnectorListener = new CemEdgeConnectorListener(this);
            
            //TODO: SetupZoom(ContentZoomer.DefaultMinScale * 0.5f, ContentZoomer.DefaultMaxScale);

            CreateDefaultElements();

            RegisterCallbacks();
            SetupManipulators();

            LoadStyleSheets();

            CreateSearch();
        }

        #region Initialization

        void RegisterCallbacks()
        {
            RegisterCallback<GeometryChangedEvent>(GeometryChangedCallback);
            RegisterCallback<KeyUpEvent>(OnKeyUp);

            graphViewChanged = OnGraphViewChanged;
            viewTransformChanged = OnViewTransformChanged;
        }

        private void CreateDefaultElements()
        {
            Insert(0, new GridBackground() { name = "Grid" });
            Add(_minimap = new MiniMap { anchored = true, maxWidth = 200, maxHeight = 100, visible = false });
        }

        private void LoadStyleSheets()
        {
            styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>("Assets/CemWindow.uss"));
        }

        private void SetupManipulators()
        {
            this.AddManipulator(new ContentZoomer());
            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());
        }

        private void CreateSearch()
        {
            _searchProvider = ScriptableObject.CreateInstance<CemSearchProvider>();
            _searchProvider.Initialize(this);
            nodeCreationRequest = ctx =>
                SearchWindow.Open(new SearchWindowContext(ctx.screenMousePosition), _searchProvider);
        }

        #endregion

        #region Callbacks

        private void OnViewTransformChanged(GraphView graphview)
        {
            
        }

        private void GeometryChangedCallback(GeometryChangedEvent evt)
        {
            _minimap.SetPosition(new Rect(worldBound.width - 205, 25, 200, 100));
        }

        private void OnKeyUp(KeyUpEvent evt)
        {
            if (evt.target != this)
            {
                return;
            }

            switch (evt.keyCode)
            {
                case KeyCode.C when !evt.ctrlKey && !evt.commandKey:
                    // AddComment();
                    break;
                case KeyCode.M:
                    _minimap.visible = !_minimap.visible;
                    break;
                case KeyCode.H when !evt.ctrlKey && !evt.commandKey:
                    HorizontallyAlignSelectedNodes();
                    break;
                case KeyCode.V when !evt.ctrlKey && !evt.commandKey:
                    VerticallyAlignSelectedNodes();
                    break;
            }
        }

        private void HorizontallyAlignSelectedNodes()
        {
            float sum = 0;
            int count = 0;

            // TODO: Implement a way to Align to "first selected" thing rather then average
            foreach (var selectable in selection)
            {
                if (selectable is Node node)
                {
                    sum += node.GetPosition().xMin;
                    count++;
                }
            }

            float xAvg = sum / count;
            foreach (var selectable in selection)
            {
                if (selectable is INodeView { IsMovable: true } node)
                {
                    var pos = node.GetPosition();
                    pos.xMin = xAvg;
                    node.SetPosition(pos);
                }
            }
        }

        private void VerticallyAlignSelectedNodes()
        {
            float sum = 0;
            int count = 0;

            foreach (var selectable in selection)
            {
                if (selectable is INodeView { IsMovable: true } node)
                {
                    sum += node.GetPosition().yMin;
                    count++;
                }
            }

            float yAvg = sum / count;
            foreach (var selectable in selection)
            {
                if (selectable is Node node)
                {
                    var pos = node.GetPosition();
                    pos.yMin = yAvg;
                    node.SetPosition(pos);
                }
            }
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            var changeMade = false;
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    switch (element)
                    {
                        case INodeView view:
                            changeMade = true;
                            /* TODO
                            view.FlowInPortContainer.Query<Port>().ForEach(CleanupFlowConnectionElements);
                            view.FlowOutPortContainer.Query<Port>().ForEach(CleanupFlowConnectionElements);
                            */
                            Graph.Remove(view.Node);
                            break;
                        case Edge edge:
                            changeMade = true;
                            Graph.Disconnect((IPort)edge.output.userData, (IPort)edge.input.userData);
                            break;
                        default:
                            Debug.LogWarning($"Unhandled GraphElement Removed: {element.GetType().FullName} | {element.name} | {element.title}");
                            break;
                    }
                }
            }
            
            if (change.movedElements != null)
            {
                foreach (var element in change.movedElements)
                {
                    switch (element)
                    {
                        case INodeView view:
                            changeMade = true; 
                            view.Node.NodeRect = view.GetPosition();
                            break;
                        default:
                            Debug.LogWarning($"Unhandled GraphElement Moved: {element.GetType().FullName} | {element.name} | {element.title}");
                            break;
                    }
                }
            }
            
            if (change.edgesToCreate != null)
            {
                foreach (var edge in change.edgesToCreate)
                {
                    changeMade = true;
                    Graph.Connect((IPort)edge.output.userData, (IPort)edge.input.userData);
                }
            }

            if (changeMade)
            {
                Save();
                Graph.MarkDirty();
            }
            
            return change;
        }

        private void CleanupFlowConnectionElements(Port port)
        {
            foreach (Edge connection in new List<Edge>(port.connections))
            {
                if ((connection.capabilities & Capabilities.Deletable) != 0)
                {
                    TODO: Graph.Disconnect((IPort) connection.output.userData, (IPort) connection.input.userData);
                    
                    // Replicate what Unity is doing in their "DeleteElement" method
                    connection.output.Disconnect(connection);
                    connection.input.Disconnect(connection);
                    // connection.output = null;
                    // connection.input = null;
                    RemoveElement(connection);
                }
            }
        }

        #endregion

        #region Modifications

        public void CreateNode(Model.INodeFactory nodeFactory, Vector2 position)
        {
            var node = Graph.Create(nodeFactory);
            var window = EditorWindow.GetWindow<CemWindow>();
            node.NodePosition = window.rootVisualElement.ChangeCoordinatesTo(contentViewContainer, position - window.position.position - new Vector2(3, 26));
            CreateNodeView(node);
            Save();
        }
        
        INodeView CreateNodeView(Model.INode node)
        {
            var element = new CemNodeView(node);
            if (element is IEditorNodeView editorView) editorView.EdgeListener = EdgeConnectorListener;
            element.SetPosition(node.NodeRect);
            AddElement(element);
            return element;
        }
        
        private void CreateNodeViews()
        {
            var d = Graph.Nodes.Select(CreateNodeView).ToList();
            foreach (var view in d) 
                CreateConnections(view);
        }

        private void CreateConnections(INodeView nodeView)
        {
            var node = nodeView.Node;
            // TODO: create connections
            
            /*
        foreach (var flowOut in node.FlowOutPorts)
        {
            foreach (var connection in Graph.FlowOutConnections.SafeGet(flowOut.Id))
            {
                if (!_nodeViewCache.TryGetValue(connection.Node, out var inputView))
                {
                    Debug.Log($"Unable To Find Node View for {connection.Node}");
                    continue;
                }
                var inputPort = inputView.FlowInPortContainer.Q<PortView>(connection.Port);
                var outputPort = view.FlowOutPortContainer.Q<PortView>(flowOut.Id.Port);
                if (inputPort != null && outputPort != null)
                    AddElement(outputPort.ConnectTo(inputPort));
                else
                    Debug.Log($"Unable To Make a Flow Port Connection | {flowOut} => {connection.Node}.{connection.Port}");
            }
        }
            */
            
        }

        #endregion

        #region Logic
        
        void Save()
        {
            /*TODO
            if (GraphAsset is ScriptableObject so && so != null)
            {
                // TODO: Purge Keys from connections tables that have no values in their port collection?
                EditorUtility.SetDirty(so);
                AssetDatabase.SaveAssets();
            }
            */
        }

        public void Reload()
        {
            Cleanup();
            
            /*TODO if (GraphAsset == null) return;
            Graph.Definition(Graph);
            */
            CreateSearch();
            // TODO: If graph.NodeCount > 100 we need a loading bar and maybe an async process that does the below
            // https://docs.unity3d.com/ScriptReference/EditorUtility.DisplayProgressBar.html
            CreateNodeViews();
        }

        protected void Cleanup()
        {
            graphViewChanged = null;
            DeleteElements(graphElements.ToList());
            graphViewChanged = OnGraphViewChanged;
        }

        internal void OpenSearch(Vector2 screenPosition, CemPortView port)
        {
            //TODO: port doing
            SearchWindow.Open(new SearchWindowContext(screenPosition), _searchProvider);
        }

        public override List<Port> GetCompatiblePorts(Port sp, NodeAdapter nodeAdapter)
        {
            var compatible = new List<Port>();

            /* TODO
            var startPort = (CemPortView)sp;
            ports.ForEach(x =>
            {
                var port = (CemPortView)x;
                if(startPort == port) return;
                if(startPort.node == port.node) return;
                if(startPort.direction == port.direction) return;
                
                if(port.direction == Direction.Input && !port.portType.IsAssignableFrom(startPort.portType)) return;
                if(port.direction == Direction.Output && !startPort.portType.IsAssignableFrom(port.portType)) return;
                
                compatible.Add(port);
            });
            */
            
            return compatible;
        }

        #endregion
    }
}