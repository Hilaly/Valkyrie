using System;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie
{
    public interface IPort
    {
        string Uid { get; }
        Direction Direction { get; }
        Port.Capacity Capacity { get; }
        Type ValueType { get; }
        string Name { get; }
        Orientation Orientation { get; }
    }
    
    public interface IValuePort : IPort
    {}
    public interface IFlowPort : IPort
    {}

    public interface IGraph
    {
        string Uid { get; }
        int NodeCount { get; }
        IEnumerable<INode> Nodes { get; }
        T Add<T>(T node) where T : INode;
        void Remove(INode node);
        void Disconnect(IPort outputPort, IPort inputPort);
        void Connect(IPort outputPort, IPort inputPort);
        void MarkDirty();
    }

    class KeyListCollection<TKey, TValue> : Dictionary<TKey, List<TValue>>
    {
        public void Connect(TKey key, TValue value)
        {
            if (TryGetValue(key, out var list))
                list.Add(value);
            else
                Add(key, new List<TValue>() { value });
        }

        public void Disconnect(TKey key, TValue value)
        {
            if (TryGetValue(key, out var list))
                list.Remove(value);
        }
    }

    [Serializable]
    public class Graph : IGraph
    {
        [SerializeField, HideInInspector] private string _uid;
        [SerializeReference] private List<INode> _nodes = new();
        [SerializeReference] private KeyListCollection<string, string> valueInConnections = new();
        [SerializeReference] private KeyListCollection<string, string> flowOutConnections = new();

        public string Uid => _uid;
        
        public int NodeCount => _nodes.Count;
        public IEnumerable<INode> Nodes => _nodes;
        
        public T Add<T>(T node) where T : INode
        {
            _nodes.Add(node);
            node.Define(this);
            return node;
        }

        public void Remove(INode node)
        {
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                var n = _nodes[i];
                if (n.Uid == node.Uid)
                {
                    CleanupFlowPortConnections(n);
                    CleanupValuePortConnections(n);
                    _nodes.RemoveAt(i);
                }
            }
        }

        public void Clear()
        {
            _nodes = new List<INode>();
            valueInConnections.Clear();
            flowOutConnections.Clear();
            // IsDefined = false;
        }

        public void Disconnect(IPort output, IPort input)
        {
            if (input is IValuePort valueIn && output is IValuePort valueOut)
                valueInConnections.Disconnect(valueIn.Uid, valueOut.Uid);
            if (input is IFlowPort flowIn && output is IFlowPort flowOut)
                flowOutConnections.Disconnect(flowOut.Uid, flowIn.Uid);
        }

        public void Connect(IPort output, IPort input)
        {
            // TODO: if we can figure out what node each port came from we can stop storing the node ID with the port
            if (input is IValuePort valueIn && output is IValuePort valueOut)
                valueInConnections.Connect(valueIn.Uid, valueOut.Uid);
            if (input is IFlowPort flowIn && output is IFlowPort flowOut) 
                flowOutConnections.Connect(flowOut.Uid, flowIn.Uid);
        }

        public void MarkDirty()
        {
            throw new NotImplementedException();
        }
        
        
        private void CleanupFlowPortConnections(INode target)
        {
            foreach (var port in target.FlowOutPorts.Values)
            {
                flowOutConnections.Remove(port.Uid);
            }
        }

        private void CleanupValuePortConnections(INode target)
        {
            foreach (var port in target.ValueInPorts.Values)
            {
                valueInConnections.Remove(port.Uid);
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Uid.Substring(0,8)})";
        }
    }

    [Node()]
    class TypeDefineNode : BaseNode
    {
        public BaseType BaseType { get; }

        public TypeDefineNode(BaseType baseType)
        {
            BaseType = baseType;
        }

        void UpdateNodeData()
        {
            if(!FlowInPorts.TryGetValue("Parents", out _))
                FlowInPorts.Add("Parents", new CustomFlowPort()
                {
                    Capacity = Port.Capacity.Multi,
                    Direction = Direction.Input,
                    Name = "Parents",
                    Orientation = Orientation.Vertical,
                    ValueType = typeof(IInheritance)
                });
            
            if(!FlowOutPorts.TryGetValue("Children", out _))
                FlowOutPorts.Add("Children", new CustomFlowPort()
                {
                    Capacity = Port.Capacity.Multi,
                    Direction = Direction.Output,
                    Name = "Children",
                    Orientation = Orientation.Vertical,
                    ValueType = typeof(IInheritance)
                });
        }

        public override string Uid => BaseType.Uid;
    }

    internal abstract class CustomPort : IPort
    {
        public string Uid { get; set; } = Guid.NewGuid().ToString();
        
        public Direction Direction { get; set; }
        public Port.Capacity Capacity { get; set; }
        public Type ValueType { get; set; }
        public string Name { get; set; }
        public Orientation Orientation { get; set; }
    }
    internal class CustomValuePort : CustomPort, IValuePort
    {}
    internal class CustomFlowPort : CustomPort, IFlowPort
    {}
    

    public interface IInheritance
    {
    }
}