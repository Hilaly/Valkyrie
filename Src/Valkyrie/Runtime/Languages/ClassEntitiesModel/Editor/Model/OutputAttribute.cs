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

        public InputAttribute(string name)
        {
            Name = name;
        }
    }
}