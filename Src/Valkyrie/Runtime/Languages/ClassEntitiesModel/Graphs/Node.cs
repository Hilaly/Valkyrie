using System;
using System.Collections.Generic;
using UnityEngine;
using Utils;

namespace Valkyrie
{
    public interface INode
    {
        IGraph Graph { get; }
        string Uid { get; }

        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }

        Dictionary<string, IPort> ValueInPorts { get; }
        Dictionary<string, IPort> ValueOutPorts { get; }
        Dictionary<string, IPort> FlowInPorts { get; }
        Dictionary<string, IPort> FlowOutPorts { get; }
        
        IReflectionData GetData();
        void Define(IGraph graph);
    }

    [Serializable]
    public abstract class BaseNode : INode
    {
        [SerializeField] private string uId = Guid.NewGuid().ToString();
        [SerializeField] private Rect nodeRect;
        
        public IGraph Graph { get; private set; }

        public virtual string Uid => uId;
        
        public Rect NodeRect
        {
            get => nodeRect;
            set => nodeRect = value;
        }

        public Vector2 NodePosition
        {
            get => nodeRect.position;
            set => nodeRect.position = value;
        }

        public Dictionary<string, IPort> ValueInPorts { get; } = new();
        public Dictionary<string, IPort> ValueOutPorts { get; } = new();
        public Dictionary<string, IPort> FlowInPorts { get; } = new();
        public Dictionary<string, IPort> FlowOutPorts { get; } = new();
        
        public virtual IReflectionData GetData() => GetType().GetCustomAttribute<NodeAttribute>(true);

        public void Define(IGraph graph)
        {
            Graph = graph;

            DefineValuePorts();
            DefineFlowPorts();
            FinishDefine();
        }

        protected virtual void DefineValuePorts()
        {
            /* TODO
            if (!NodeAttribute.Cache.TryGet(GetType(), out var data)) return;
            foreach (var valuePort in data.ValuePorts)
            {
                var port = valuePort.GetOrCreatePort(this);
                if (valuePort.GraphPort)
                {
                    var graphPort = port.Clone(Graph);
                    (valuePort.Direction == PortDirection.Input ? Graph.ValueOutPorts : Graph.ValueInPorts).Add(graphPort.Name, graphPort);
                }
                (valuePort.Direction == PortDirection.Input ? ValueInPorts : ValueOutPorts).Add(valuePort.Name, port);
                // Debug.Log($"{this} has Value Port '{valuePort.Name} | {valuePort.Direction}'");
            }
            */
        }
        
        protected virtual void DefineFlowPorts()
        {
            /* TODO
            FlowInPorts = new Dictionary<string, IFlowPort>();
            FlowOutPorts = new Dictionary<string, IFlowPort>();
            if (!NodeAttribute.Cache.TryGet(GetType(), out var data)) return;
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
}