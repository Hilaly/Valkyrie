using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Utils;

namespace Valkyrie
{
    public static class GraphSerializer
    {
        public static JsonSerializerSettings SerSettings = new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
            TypeNameHandling = TypeNameHandling.All
        };
        
        internal static void Deserialize(this Graph graph, string json)
        {
            var jo = JObject.Parse(json);

            graph.Uid = jo.Value<string>("uid");
            foreach (JToken token in jo["nodes"])
            {
                //graph.NodesList.Add(DeserializeNode(token));
            }
        }
        
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
                ["flowIn"] = Serialize(node.FlowInPorts),
                ["flowOut"] = Serialize(node.FlowOutPorts),
                ["input"] = Serialize(node.ValueInPorts),
                ["output"] = Serialize(node.ValueOutPorts)
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