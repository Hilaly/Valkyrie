using System.Linq;
using Valkyrie.Model;

namespace Valkyrie.Utils
{
    public static class CemUtils
    {
        public static INode GetNode(this IGraph graph, IPort port) => graph.GetNode(port.Uid);

        private static INode GetNode(this IGraph graph, string portUid) =>
            graph.Nodes.FirstOrDefault(node => node.Ports.Any(x => x.Uid == portUid));
    }
}