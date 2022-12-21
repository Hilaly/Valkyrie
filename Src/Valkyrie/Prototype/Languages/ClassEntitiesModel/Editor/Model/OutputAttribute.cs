using System;
using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
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
}