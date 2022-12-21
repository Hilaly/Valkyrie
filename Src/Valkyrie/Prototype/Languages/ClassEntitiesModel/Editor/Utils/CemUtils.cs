using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Model;
using Valkyrie.Model.Nodes;

namespace Valkyrie.Utils
{
    public static class CemUtils
    {
        public static INode GetNode(this IGraph graph, IPort port) => graph.GetNode(port.Uid);

        private static INode GetNode(this IGraph graph, string portUid) =>
            graph.Nodes.FirstOrDefault(node => node.Ports.Any(x => x.Uid == portUid));

        public static IEnumerable<INode> GetPreviousNodes(this IGraph graph, IPort inputPort)
        {
            var connections = graph.GetInputConnections(inputPort.Uid);
            return connections.Select(graph.GetNode);
        }

        public static IReadOnlyList<IGraph> GetGraphTree(this IGraph graph)
        {
            var graphList = new List<IGraph>();
            var g = graph;
            while (g != null)
            {
                graphList.Add(g);
                g = g is INode node ? node.Graph : default;
            }

            return graphList;
        }

        public static IGraph GetGraphRoot(this IGraph graph) => graph.GetGraphTree().Last();

        public static IEnumerable<T> GetAllSubNodes<T>(this IGraph graph)
        {
            foreach (var node in graph.Nodes)
            {
                if (node is T r)
                    yield return r;
                if(node is IGraph g)
                    foreach (var subNode in g.GetAllSubNodes<T>())
                        yield return subNode;
            }
        }

        public static Type GetTypedValue(this IGraph graph, string typeName)
        {
            var type = typeName.FindType(false);
            if (type != null)
                return type;
            var root = graph.GetGraphRoot();
            foreach (var node in root.GetAllSubNodes<ITypeDefine>())
            {
                if (node.Name == typeName)
                    return node.GetType();
            }

            Debug.LogWarning($"[CEM]: {typeName} is not valid");
            
            return null;
        }
    }
}