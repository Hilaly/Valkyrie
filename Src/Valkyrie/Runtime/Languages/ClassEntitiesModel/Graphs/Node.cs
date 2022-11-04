using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie
{
    [Serializable]
    public abstract class BaseNode : INode
    {
        [SerializeField, JsonProperty] private string uId = Guid.NewGuid().ToString();
        [SerializeField] private Rect nodeRect;
        [SerializeField, JsonProperty] private List<IValuePort> valueInPorts = new();
        [SerializeField, JsonProperty] private List<IValuePort> valueOutPorts = new();
        [SerializeField, JsonProperty] private List<IFlowPort> flowInPorts = new();
        [SerializeField, JsonProperty] private List<IFlowPort> flowOutPorts = new();
        
        [JsonIgnore] public IGraph Graph { get; private set; }

        [JsonIgnore] public virtual string Uid => uId;
        
        [JsonIgnore] public Rect NodeRect
        {
            get => nodeRect;
            set => nodeRect = value;
        }

        public Vector2 NodePosition
        {
            get => nodeRect.position;
            set => nodeRect.position = value;
        }
        public Vector2 NodeSize
        {
            get => nodeRect.size;
            set => nodeRect.size = value;
        }

        [JsonIgnore] public IReadOnlyList<IValuePort> ValueInPorts => valueInPorts;
        [JsonIgnore] public IReadOnlyList<IValuePort> ValueOutPorts => valueOutPorts;
        [JsonIgnore] public IReadOnlyList<IFlowPort> FlowInPorts => flowInPorts;
        [JsonIgnore] public IReadOnlyList<IFlowPort> FlowOutPorts => flowOutPorts;

        public virtual INodeFactory GetData() => default;

        public void Define(IGraph graph)
        {
            Graph = graph;

            DefineValuePorts();
            DefineFlowPorts();
            FinishDefine();
        }

        protected virtual void DefineValuePorts()
        {
            var data = GetData();
            if (data == null) return;
                /* TODO
            foreach (var valuePort in data.ValuePorts)
            {
                var port = valuePort.GetOrCreatePort(this);
                if (valuePort.GraphPort)
                {
                    var graphPort = port.Clone(Graph);
                    (valuePort.Direction == PortDirection.Input ? Graph.ValueOutPorts : Graph.ValueInPorts).Add(graphPort.Name, graphPort);
                }
                (valuePort.Direction == Direction.Input ? valueInPorts : valueOutPorts).Add(port);
            }
                */
        }
        
        protected virtual void DefineFlowPorts()
        {
            var data = GetData();
            if (data == null) return;
            /* TODO
            FlowInPorts = new Dictionary<string, IFlowPort>();
            FlowOutPorts = new Dictionary<string, IFlowPort>();
            foreach (var flowPort in data.FlowPorts)
            {
                var port = flowPort.GetOrCreatePort(this);
                if (flowPort.GraphPort)
                {
                    var graphPort = port.Clone(Graph);
                    (flowPort.Direction == PortDirection.Input ? Graph.FlowOutPorts : Graph.FlowInPorts).Add(graphPort.Name, graphPort);
                }
                (flowPort.Direction == PortDirection.Input ? FlowInPorts : FlowOutPorts).Add(flowPort.Name, port);
                // Debug.Log($"{this} has Flow Port '{flowPort.Name} | {flowPort.Direction}'");
            }
            */
        }

        protected virtual void FinishDefine() {}

        public override string ToString() => $"{GetType().Name}({Uid[..8]})";
    }
    
    public static class PortAttributeExtensions
    {
        public static IPort GetOrCreatePort(this IPortAttribute self, INode node)
        {
            var port = (IPort)self.Info.GetValue(node);
            if (port == null)
            {
                port = (IPort)Activator.CreateInstance(self.Info.FieldType);
                self.Info.SetValue(node, port);
            }
            return port;
        }
    }
    
    public static class ValueAttributeExtensions
    {
        public static IValuePort GetOrCreatePort(this IValuePortAttribute self, INode node)
        {
            var port = self.Info.GetValue(node) as IValuePort ?? ValuePort.MakeGeneric(self);
            port.Definition(node, self);
            return port;
        }
    }
}