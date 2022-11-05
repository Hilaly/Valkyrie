using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie.Model
{
    [Serializable]
    public abstract class CemGraph : CemNode, IGraph
    {
        [SerializeField, JsonProperty] private List<INode> _nodes = new();
        [SerializeField, JsonProperty] private KeyListCollection<string, string> _connections = new();

        [JsonIgnore] public IReadOnlyList<INode> Nodes => _nodes;

        public virtual void MarkDirty()
        {
            Debug.Log($"[CEM] graph {Uid} changed");
        }

        public abstract IEnumerable<INodeFactory> GetFactories();
        public IEnumerable<string> GetOutputConnections(string outputPortUid)
        {
            foreach (var (key, list) in _connections)
                if (list.Contains(outputPortUid))
                    yield return key;
        }

        public IEnumerable<string> GetInputConnections(string inputPortUid) =>
            _connections.TryGetValue(inputPortUid, out var list)
                ? list
                : Enumerable.Empty<string>();

        public INode Create(INodeFactory nodeType)
        {
            var node = nodeType.Create();
            _nodes.Add(node);
            if(node is INodeExt nodeExt)
                nodeExt.OnCreate();
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
                    CleanupConnections(n);
                    _nodes.RemoveAt(i);
                    changed = true;
                }
            }

            if (changed)
                MarkDirty();
        }

        public void Clear()
        {
            _nodes.Clear();
            _connections.Clear();
            MarkDirty();
        }

        public void Disconnect(IPort output, IPort input)
        {
            _connections.Disconnect(input.Uid, output.Uid);
            MarkDirty();
        }

        public void Connect(IPort output, IPort input)
        {
            _connections.Connect(input.Uid, output.Uid);
            MarkDirty();
        }

        private void CleanupConnections(INode node)
        {
            var keys = _connections.Keys.Where(x => x.StartsWith(node.Uid)).ToList();
            keys.ForEach(x => _connections.Remove(x));
            var values = _connections.Where(x => x.Value.Any(u => u.StartsWith(node.Uid)))
                .SelectMany(x =>
                    x.Value.Where(u => u.StartsWith(node.Uid)).Select(u => new KeyValuePair<string, string>(x.Key, u)))
                .ToList();
            values.ForEach(x => _connections.Disconnect(x.Key, x.Value));
        }
    }
}