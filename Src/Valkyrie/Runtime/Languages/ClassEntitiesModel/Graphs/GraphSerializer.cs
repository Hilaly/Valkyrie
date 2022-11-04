using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using Utils;

namespace Valkyrie
{
    public static class GraphSerializer
    {
        public static string Serialize(this IGraph graph)
        {
            var nodesArray = new JArray();
            foreach (var graphNode in graph.Nodes) 
                nodesArray.Add(Serialize(graphNode));
            var result = new JObject
            {
                ["uid"] = graph.Uid,
                ["nodes"] = nodesArray
            };
            return result.ToString();
        }

        private static JObject Serialize(INode node)
        {
            var jo = new JObject
            {
                ["$type"] = node.GetType().FullName,
                ["uid"] = node.Uid,
                ["rect"] = node.NodeRect.ToJObject(),
                ["flowIn"] = Serialize(node.FlowInPorts.Values),
                ["flowOut"] = Serialize(node.FlowOutPorts.Values),
                ["input"] = Serialize(node.ValueInPorts.Values),
                ["output"] = Serialize(node.ValueOutPorts.Values)
            };
            return jo;
        }

        private static JToken Serialize(IEnumerable<IPort> ports)
        {
            var result = new JArray();
            foreach (var port in ports)
            {
                result.Add(new JObject()
                {
                    ["$type"] = port.GetType().FullName,
                    ["uid"] = port.Uid,
                    ["name"] = port.Name,
                    ["capacity"] = port.Capacity.ToString(),
                    ["direction"] = port.Direction.ToString(),
                    ["orientation"] = port.Orientation.ToString(),
                    ["valueType"] = port.ValueType.FullName
                });
            }

            return result;
        }
    }
}