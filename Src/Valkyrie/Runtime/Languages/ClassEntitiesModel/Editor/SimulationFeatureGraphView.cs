using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.Editor.ClassEntitiesModel
{
    class SimulationFeatureGraphView : BaseGraphView
    {
        public override List<Port> GetCompatiblePorts(Port sp, NodeAdapter nodeAdapter)
        {
            var startPort = (CemPortView)sp;
            var compatible = new List<Port>();

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
            
            return compatible;
        }

        private GraphViewChange OnGraphViewChanged(GraphViewChange change)
        {
            RecordUndo("Graph Edit");
            bool changeMade = false;
            if (change.elementsToRemove != null)
            {
                foreach (var element in change.elementsToRemove)
                {
                    switch (element)
                    {
                        case INodeView view:
                            changeMade = true;
                            view.FlowInPortContainer.Query<Port>().ForEach(CleanupFlowConnectionElements);
                            view.FlowOutPortContainer.Query<Port>().ForEach(CleanupFlowConnectionElements);
                            _nodeViewCache.Remove(view.Node.Uid);
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
                    Debug.Log($"edge to create");
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
        
        private void OnViewTransformChanged(GraphView graphview)
        {
            
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
        
        public override void Save()
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

        public override void Reload()
        {
            Cleanup();
            
            /*TODO if (GraphAsset == null) return;
            Graph.Definition(Graph);
            */
            _nodeViewCache = new Dictionary<string, INodeView>(Graph.NodeCount);
            CreateSearch();
            // TODO: If graph.NodeCount > 100 we need a loading bar and maybe an async process that does the below
            // https://docs.unity3d.com/ScriptReference/EditorUtility.DisplayProgressBar.html
            CreateNodeViews();
            CreateConnections();
        }
        
        protected void Cleanup()
        {
            graphViewChanged = null;
            DeleteElements(graphElements.ToList());
            graphViewChanged = OnGraphViewChanged;
        }

        private void CreateNodeViews()
        {
            foreach (var node in Graph.Nodes) 
                CreateNodeView<CemNodeView>(node, node.GetData());
        }

        private void CreateConnections()
        {
            foreach (var node in Graph.Nodes)
            {
                var view = _nodeViewCache[node.Uid];
                CreateValueConnections(view, node);
                CreateFlowConnections(view, node);
            }
        }

        private void CreateValueConnections(INodeView view, INode node)
        {
            foreach (var valueIn in node.ValueInPorts)
            {
                /*
                foreach (var connection in Graph.ValueInConnections.SafeGet(valueIn.Id))
                {
                    if (!_nodeViewCache.TryGetValue(connection.Node, out var outputView)) continue;
                    var inputPort = view.ValueInPortContainer.Q<PortView>(valueIn.Id.Port);
                    var outputPort = outputView.ValueOutPortContainer.Q<PortView>(connection.Port);
                    if (inputPort != null && outputPort != null)
                        AddElement(outputPort.ConnectTo(inputPort));
                }
                */
            }
        }

        private void CreateFlowConnections(INodeView view, INode node)
        {
            foreach (var flowOut in node.FlowOutPorts)
            {
                /*
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
                */
            }
        }

    }
}