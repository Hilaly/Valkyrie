using System;
using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
    public interface IPortAttribute
    {
        string Name { get; }
        Direction Direction { get; }
    }
    
    [AttributeUsage(AttributeTargets.Property)]
    public class OutputAttribute : Attribute, IPortAttribute
    {
        public string Name { get; }
        public Direction Direction => Direction.Output;
        public Port.Capacity Capacity { get; set; } = Port.Capacity.Single;

        public OutputAttribute(string name)
        {
            Name = name;
        }
    }
    [AttributeUsage(AttributeTargets.Property)]
    public class InputAttribute : Attribute, IPortAttribute
    {
        public string Name { get; }
        public Direction Direction => Direction.Input;
        public Port.Capacity Capacity { get; set; } = Port.Capacity.Single;

        public InputAttribute(string name)
        {
            Name = name;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class ExportPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsOnProperty : Attribute
    {
        public string PropertyName { get; }

        public DependsOnProperty(string propertyName)
        {
            PropertyName = propertyName;
        }
    }

    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsOnInput : Attribute
    {
        public string InputName { get; }

        public DependsOnInput(string inputPortName)
        {
            InputName = inputPortName;
        }
    }
}