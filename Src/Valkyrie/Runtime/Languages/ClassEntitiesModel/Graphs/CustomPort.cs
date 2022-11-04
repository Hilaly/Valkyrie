using System;
using Newtonsoft.Json;
using UnityEditor.Experimental.GraphView;
using UnityEngine.Scripting;

namespace Valkyrie
{
    internal abstract class BasePort : IPort
    {
        public string Uid { get; set; } = Guid.NewGuid().ToString();

        [JsonIgnore] public IGraph Graph => Node.Graph;
        [JsonIgnore] public INode Node { get; internal set; }
        public string Name { get; internal set; }
        
        public Direction Direction { get; set; }
        public Port.Capacity Capacity { get; set; }
        
        [JsonIgnore] public Type ValueType { get; set; }
        public Orientation Orientation { get; set; }
    }

    [Preserve]
    class ValuePort : BasePort, IValuePort
    {
        [JsonIgnore] public IValuePortAttribute Info { get; private set; }
        
        public void Definition(INode node, IValuePortAttribute info)
        {
            Node = node;
            Info = info;
            
            Uid = $"{node.Uid}.{info.Name}";
            Node = node;
            Name = info.Name;
            Direction = info.Direction;
            Capacity = info.Capacity;
            ValueType = info.Info.FieldType;
            Orientation = info.Orientation;
        }

        internal static ValuePort MakeGeneric(IValuePortAttribute info)
        {
            var fieldType = info.Info.FieldType;
            var type = typeof(GenericValuePort<>).MakeGenericType(fieldType);
            var result = (ValuePort)Activator.CreateInstance(type);
            return result;
        }
    }

    [Preserve]
    class GenericValuePort<T> : ValuePort
    {
    }

    [Preserve]
    internal class ValuePort<T> : BasePort, IValuePort
    {
        public void Definition(INode node, IValuePortAttribute info)
        {
            Uid = $"{node.Uid}.{info.Name}";
            Node = node;
            Name = info.Name;
            Direction = info.Direction;
            Capacity = info.Capacity;
            ValueType = typeof(T);
        }
        
        [Preserve]
        public ValuePort()
        {
            Value = default;
        }

        public T Value { get; set; }
    }

    internal class FlowPort : BasePort, IFlowPort
    {
    }
}