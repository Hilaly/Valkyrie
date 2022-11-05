using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie.Model
{
    [Serializable]
    public class CemNode : INodeExt
    {
        [SerializeField, JsonProperty] private string uid = Guid.NewGuid().ToString();
        [SerializeField, JsonProperty] private Rect rect;
        [SerializeField, JsonProperty] private Dictionary<string, CemPort> ports = new();

        public event Action<CemNodeChangedEvent> NodeChanged;

        public string Name { get; set; }

        [JsonIgnore] public string Uid => uid;

        [JsonIgnore]
        public Rect NodeRect
        {
            get => rect;
            set => rect = value;
        }

        [JsonIgnore]
        public Vector2 NodePosition
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
            OnNodeChanged(CemNodeChangedEvent.AddPort(port));
            return port;
        }

        protected void RemovePort(IPort port)
        {
            if (ports.Remove(port.Name))
                OnNodeChanged(CemNodeChangedEvent.RemovePort(port));
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

        public virtual void OnCreate()
        {
        }

        #endregion

        protected virtual void OnNodeChanged(CemNodeChangedEvent obj)
        {
            var l = NodeChanged;
            l?.Invoke(obj);
        }
    }
}