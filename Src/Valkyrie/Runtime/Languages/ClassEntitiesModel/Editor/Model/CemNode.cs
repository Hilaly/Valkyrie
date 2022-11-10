using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace Valkyrie.Model
{
    [Serializable]
    public class CemNode : INodeWithFields, INodeClone
    {
        [SerializeField, JsonProperty] private string uid = Guid.NewGuid().ToString();
        [SerializeField, JsonProperty] private Rect rect;
        [SerializeField, JsonProperty] private Dictionary<string, CemPort> ports = new();
        [SerializeField, JsonProperty] private string _name;
        private List<INodeProperty> properties = new();

        public event Action<CemNodeChangedEvent> NodeChanged;

        public IGraph Graph { get; set; }

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

        public CemNode()
        {
            InitAttributesProperties();
            SubscribeToInputChangedEvents();
            SubscribeToPropertiesChangedEvents();
        }

        public IPort GetPort(string name) => ports.SingleOrDefault(x => x.Value.Name == name).Value;

        protected IPort CreatePort<T>(string name) where T : CemPort => CreatePort(name, typeof(T), Port.Capacity.Single);

        private IPort CreatePort(string name, Type portConcreteType, Port.Capacity capacity)
        {
            var port = (CemPort)Activator.CreateInstance(portConcreteType);
            port.Capacity = capacity;
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

        protected IPort CreatePort(string name, Type valueType, Direction direction, Port.Capacity capacity)
        {
            var genericType = direction == Direction.Input
                ? typeof(CemInputPort<>)
                : typeof(CemOutputPort<>);
            var portType = genericType.MakeGenericType(valueType);
            return CreatePort(name, portType, capacity);
        }

        protected INodeProperty CreateProperty(string propertyName, string displayName,
            Action<INodeProperty> callOnChanged) =>
            CreateProperty(GetType().GetProperty(propertyName), displayName, callOnChanged);

        protected INodeProperty CreateProperty(PropertyInfo propertyInfo, string name,
            Action<INodeProperty> callOnChanged)
        {
            var r = new PropertyInfoNodeProperty(this, propertyInfo, name ?? propertyInfo.Name, callOnChanged);
            properties.Add(r);
            return r;
        }

        public virtual void PrepareForDrawing()
        {
            EnsureNodesExist();
            EnsurePortsExists();
        }

        [OnDeserialized]
        internal void OnDeserializedMethod(StreamingContext context)
        {
            EnsureNodesExist();
            foreach (var pair in ports)
                pair.Value.Node = this;
            SubscribeToInputChangedEvents();
            SubscribeToPropertiesChangedEvents();
        }

        protected virtual void EnsureNodesExist()
        {
        }

        protected virtual void EnsurePortsExists()
        {
            Collect<OutputAttribute>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty,
                (info, attr) =>
                {
                    if(GetPort(attr.Name) == null)
                        CreatePort(attr.Name, info.PropertyType, attr.Direction, attr.Capacity);
                });
            Collect<InputAttribute>(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty,
                (info, attr) =>
                {
                    if(GetPort(attr.Name) == null)
                        CreatePort(attr.Name, info.PropertyType, attr.Direction, attr.Capacity);
                });
        }

        #region INodeExr

        void Collect<T>(BindingFlags filter, Action<PropertyInfo, T> call)
            where T : Attribute
        {
            foreach (var property in GetType()
                .GetProperties(filter)
                .Where(x => x.GetCustomAttributes<T>().Any()))
            {
                foreach (var attribute in property.GetCustomAttributes<T>())
                    call.Invoke(property, attribute);
            }
        }

        private void InitAttributesProperties()
        {
            Collect<ExportPropertyAttribute>(
                BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.SetProperty,
                (info, attr) =>
                {
                    CreateProperty(info, attr.Name ?? info.Name,
                        prop => OnNodeChanged(CemNodeChangedEvent.PropertyChanged(prop)));
                });
        }

        private void SubscribeToInputChangedEvents()
        {
            foreach (var info in GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<DependsOnInput>().Any())
                .Where(x => x.GetCustomAttributes<OutputAttribute>().Any()))
            {
                foreach (var attribute in info.GetCustomAttributes<DependsOnInput>())
                    NodeChanged += e =>
                    {
                        if (e.portValueChanged == null)
                            return;

                        var outputs = info.GetCustomAttributes<OutputAttribute>().Select(x => GetPort(x.Name)).ToList();
                        foreach (var unused in e.portValueChanged.Where(x => x.Name == attribute.InputName))
                        foreach (var port in outputs)
                            OnNodeChanged(CemNodeChangedEvent.PortValueChanged(port));
                    };
            }
        }

        private void SubscribeToPropertiesChangedEvents()
        {
            foreach (var info in GetType()
                .GetProperties(BindingFlags.Public | BindingFlags.GetProperty | BindingFlags.Instance)
                .Where(x => x.GetCustomAttributes<DependsOnProperty>().Any())
                .Where(x => x.GetCustomAttributes<OutputAttribute>().Any()))

            {
                foreach (var attribute in info.GetCustomAttributes<DependsOnProperty>())
                    NodeChanged += e =>
                    {
                        if (e.propertyChanged == null)
                            return;

                        var outputs = info.GetCustomAttributes<OutputAttribute>().Select(x => GetPort(x.Name)).ToList();
                        foreach (var unused in e.propertyChanged.Where(x => x.Name == attribute.PropertyName))
                        foreach (var port in outputs)
                            OnNodeChanged(CemNodeChangedEvent.PortValueChanged(port));
                    };
            }
        }

        #endregion

        protected virtual void OnNodeChanged(CemNodeChangedEvent obj)
        {
            var l = NodeChanged;
            l?.Invoke(obj);
        }

        public virtual void Clone()
        {
            uid = Guid.NewGuid().ToString();
            this.ports.Clear();
        }
    }
}