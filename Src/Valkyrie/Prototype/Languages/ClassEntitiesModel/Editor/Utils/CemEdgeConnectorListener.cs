using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Valkyrie.View;

namespace Valkyrie.Utils
{
    class CemEdgeConnectorListener : IEdgeConnectorListener
    {
        private readonly CemGraphView _cemGraphView;

        private readonly GraphViewChange _graphViewChange;
        private readonly List<Edge> _edgesToCreate;
        private readonly List<GraphElement> _edgesToDelete;

        public CemEdgeConnectorListener(CemGraphView cemGraphView)
        {
            _cemGraphView = cemGraphView;
            _edgesToCreate = new List<Edge>();
            _edgesToDelete = new List<GraphElement>();
            _graphViewChange.edgesToCreate = _edgesToCreate;
        }
        
        /// <summary>
        /// Activate the search dialog when an edge is dropped on an arbitrary location
        /// </summary>
        public void OnDropOutsidePort(Edge edge, Vector2 position)
        {
            var screenPosition = GUIUtility.GUIToScreenPoint(
                Event.current.mousePosition
            );
            
            if (edge.output != null)
            {
                _cemGraphView.OpenSearch(
                    screenPosition, 
                    edge.output.edgeConnector.edgeDragHelper.draggedPort as CemPortView
                );
            }
            else if (edge.input != null)
            {
                _cemGraphView.OpenSearch(
                    screenPosition, 
                    edge.input.edgeConnector.edgeDragHelper.draggedPort as CemPortView
                );
            }
        }
        
        /// <summary>
        /// Handle connecting nodes when an edge is dropped between two ports
        /// </summary>
        public void OnDrop(GraphView graphView, Edge edge)
        {
            _edgesToCreate.Clear();
            _edgesToCreate.Add(edge);
            _edgesToDelete.Clear();
            if (edge.input.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in edge.input.connections)
                {
                    if (connection != edge)
                        _edgesToDelete.Add(connection);
                }
            }
            if (edge.output.capacity == Port.Capacity.Single)
            {
                foreach (Edge connection in edge.output.connections)
                {
                    if (connection != edge)
                        _edgesToDelete.Add(connection);
                }
            }
            if (_edgesToDelete.Count > 0) graphView.DeleteElements(_edgesToDelete);
            List<Edge> edgesToCreate = _edgesToCreate;
            if (graphView.graphViewChanged != null) edgesToCreate = graphView.graphViewChanged(_graphViewChange).edgesToCreate;
            foreach (Edge edge1 in edgesToCreate)
            {
                graphView.AddElement(edge1);
                edge.input.Connect(edge1);
                edge.output.Connect(edge1);
            }
        }
    }
}