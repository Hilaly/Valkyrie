using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie
{
    [Serializable]
    public class Graph : IGraph
    {
        [SerializeField, HideInInspector, JsonProperty] private string _uid = Guid.NewGuid().ToString();
        [SerializeReference, JsonProperty] private List<INode> _nodes = new();
        [SerializeReference, JsonProperty] private KeyListCollection<string, string> valueInConnections = new();
        [SerializeReference, JsonProperty] private KeyListCollection<string, string> flowOutConnections = new();

        [JsonIgnore] public string Uid
        {
            get => _uid;
            internal set => _uid = value;
        }

        [JsonIgnore] internal List<INode> NodesList => _nodes;

        [JsonIgnore] public int NodeCount => _nodes.Count;
        [JsonIgnore] public IEnumerable<INode> Nodes => _nodes;
        
        public T Add<T>(T node) where T : INode
        {
            _nodes.Add(node);
            node.Define(this);
            MarkDirty();
            return node;
        }

        public void Remove(INode node)
        {
            bool changed = false;
            for (int i = _nodes.Count - 1; i >= 0; i--)
            {
                var n = _nodes[i];
                if (n.Uid == node.Uid)
                {
                    CleanupFlowPortConnections(n);
                    CleanupValuePortConnections(n);
                    _nodes.RemoveAt(i);
                    changed = true;
                }
            }
            if(changed)
                MarkDirty();
        }

        public void Clear()
        {
            _nodes = new List<INode>();
            valueInConnections.Clear();
            flowOutConnections.Clear();
            // IsDefined = false;
            MarkDirty();
        }

        public void Disconnect(IPort output, IPort input)
        {
            if (input is IValuePort valueIn && output is IValuePort valueOut)
            {
                valueInConnections.Disconnect(valueIn.Uid, valueOut.Uid);
                MarkDirty();
            }

            if (input is IFlowPort flowIn && output is IFlowPort flowOut)
            {
                flowOutConnections.Disconnect(flowOut.Uid, flowIn.Uid);
                MarkDirty();
            }
        }

        public void Connect(IPort output, IPort input)
        {
            // TODO: if we can figure out what node each port came from we can stop storing the node ID with the port
            if (input is IValuePort valueIn && output is IValuePort valueOut)
            {
                valueInConnections.Connect(valueIn.Uid, valueOut.Uid);
                MarkDirty();
            }

            if (input is IFlowPort flowIn && output is IFlowPort flowOut)
            {
                flowOutConnections.Connect(flowOut.Uid, flowIn.Uid);
                MarkDirty();
            }
        }

        public void MarkDirty()
        {
            var str = JsonConvert.SerializeObject(this, GraphSerializer.SerSettings);
            
            Debug.LogWarning($"Dirty:\n{str}");
            
            File.WriteAllText("Assets/graph.json", str);
        }
        
        
        private void CleanupFlowPortConnections(INode target)
        {
            foreach (var port in target.FlowOutPorts)
            {
                flowOutConnections.Remove(port.Uid);
                MarkDirty();
            }
        }

        private void CleanupValuePortConnections(INode target)
        {
            foreach (var port in target.ValueInPorts)
            {
                valueInConnections.Remove(port.Uid);
                MarkDirty();
            }
        }

        public override string ToString()
        {
            return $"{GetType().Name}({Uid.Substring(0,8)})";
        }
    }


    public interface IInheritance
    {
    }
}