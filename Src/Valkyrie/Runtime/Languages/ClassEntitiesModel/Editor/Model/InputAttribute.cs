using System;
using UnityEditor.Experimental.GraphView;

namespace Valkyrie.Model
{
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
}