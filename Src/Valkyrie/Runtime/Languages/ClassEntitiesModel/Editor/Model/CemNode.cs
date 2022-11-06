using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using Utils;

namespace Valkyrie.Model
{
    [Serializable]
    public class CemNode : INodeExt, INodeWithFields
    {
        [SerializeField, JsonProperty] private string uid = Guid.NewGuid().ToString();
        [SerializeField, JsonProperty] private Rect rect;
        [SerializeField, JsonProperty] private Dictionary<string, CemPort> ports = new();
        [SerializeField, JsonProperty] private string _name;
        private List<INodeProperty> properties = new();

        public event Action<CemNodeChangedEvent> NodeChanged;

        [JsonIgnore]
        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                    return;
                _name = value;
                OnNodeChanged(CemNodeChangedEvent.Renamed());
            }
        }

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

        [JsonIgnore] public IEnumerable<INodeProperty> Properties => properties;

        protected IPort GetPort(string name) => ports.SingleOrDefault(x => x.Value.Name == name).Value;

        protected IPort CreatePort<T>(string name) where T : CemPort => CreatePort(name, typeof(T));

        private IPort CreatePort(string name, Type portConcreteType)
        {
            var port = (CemPort)Activator.CreateInstance(portConcreteType);
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

        protected void RenamePort(IPort iPort, string newName)
        {
            if (iPort is CemPort port && ports.Remove(port.Name))
            {
                ports.Add(port.Name = newName, port);
                OnNodeChanged(CemNodeChangedEvent.PortRenamed(port));
            }
        }

        protected IPort CreateInputPort<TType>(string name) => CreatePort<CemInputPort<TType>>(name);
        protected IPort CreateOutputPort<TType>(string name) => CreatePort<CemOutputPort<TType>>(name);

        protected IPort CreatePort(string name, Type valueType, Direction direction)
        {
            var genericType = direction == Direction.Input
                ? typeof(CemInputPort<>)
                : typeof(CemOutputPort<>);
            var portType = genericType.MakeGenericType(valueType);
            return CreatePort(name, portType);
        }

        protected INodeProperty CreateProperty(string propertyName, string displayName) =>
            CreateProperty(GetType().GetProperty(propertyName), displayName);

        protected INodeProperty CreateProperty(PropertyInfo propertyInfo, string name)
        {
            var r = new PropertyInfoNodeProperty(this, propertyInfo, name ?? propertyInfo.Name);
            properties.Add(r);
            return r;
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            foreach (var pair in ports)
            {
                pair.Value.Node = this;
            }
        }

        #region INodeExr

        void Collect<T>(BindingFlags filter, Action<PropertyInfo, IPortAttribute> call)
            where T : Attribute, IPortAttribute
        {
            foreach (var property in GetType()
                .GetProperties(filter)
                .Where(x => x.GetCustomAttribute<T>() != null))
            {
                foreach (var attribute in property.GetCustomAttributes<T>())
                    call.Invoke(property, attribute);
            }
        }

        public virtual void OnCreate()
        {
            Collect<OutputAttribute>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                (info, attr) => CreatePort(attr.Name, info.PropertyType, attr.Direction));
            Collect<InputAttribute>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                (info, attr) => CreatePort(attr.Name, info.PropertyType, attr.Direction));
        }

        #endregion

        protected virtual void OnNodeChanged(CemNodeChangedEvent obj)
        {
            var l = NodeChanged;
            l?.Invoke(obj);
        }
    }

    class PropertyInfoNodeProperty : INodeProperty
    {
        private readonly INode _instance;
        private readonly PropertyInfo _info;

        public PropertyInfoNodeProperty(INode instance, PropertyInfo info, string name)
        {
            _instance = instance;
            _info = info;
            Name = name;
        }

        public string Name { get; }

        public Type PropertyType => _info.PropertyType;

        public object Value
        {
            get => _info.GetValue(_instance);
            set => _info.SetValue(_instance, value);
        }
    }
}