using System;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Valkyrie.Tools;

namespace Valkyrie.Model
{
    [Serializable]
    public abstract class CemPort : IPort
    {
        [SerializeField, JsonProperty] private string uid;
        [SerializeField, JsonProperty] private string _name;

        public Orientation Orientation { get; set; } = Orientation.Horizontal;
        public Port.Capacity Capacity { get; set; } = Port.Capacity.Single;
        
        public abstract Direction Direction { get; }

        [JsonIgnore] public string Uid
        {
            get => uid;
            set => uid = value;
        }

        [JsonIgnore] public virtual Type Type { get; set; }
        [JsonIgnore] public INode Node { get; internal set; }
        [JsonIgnore] public string Name
        {
            get => _name;
            set => _name = value;
        }

        public void Init(INode node, string name)
        {
            _name = name;
            Node = node;
            
            if (uid.IsNullOrEmpty())
                uid = $"{node.Uid}.{Guid.NewGuid()}";
        }
    }
}