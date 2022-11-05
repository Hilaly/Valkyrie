using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie.Model
{
    [Serializable]
    public class CemNode : INode, INodeExt
    {
        [SerializeField, JsonProperty] private string uid = Guid.NewGuid().ToString();
        [SerializeField, JsonProperty] private Rect rect;
        [SerializeField, JsonProperty] private Dictionary<string, CemPort> ports = new ();

        public string Name { get; set; }
        
        [JsonIgnore] public string Uid => uid;
        [JsonIgnore] public Rect NodeRect
        {
            get => rect;
            set => rect = value;
        }
        [JsonIgnore] public Vector2 NodePosition
        {
            get => rect.position;
            set => rect.position = value;
        }
        [JsonIgnore] public IEnumerable<IPort> Ports => ports.Values;

        protected IPort CreatePort<T>(string name) where T : CemPort
        {
            var port = Activator.CreateInstance<T>();
            port.Init(this, name);
            ports.Add(name, port);
            return port;
        }

        protected IPort CreateInputPort<TType>(string name) => CreatePort<CemInputPort<TType>>(name);
        protected IPort CreateOutputPort<TType>(string name) => CreatePort<CemOutputPort<TType>>(name);

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (var pair in ports)
            {
                pair.Value.Init(this, pair.Key);
            }
        }

        #region INodeExr

        public virtual void OnCreate() { }

        #endregion
    }
}