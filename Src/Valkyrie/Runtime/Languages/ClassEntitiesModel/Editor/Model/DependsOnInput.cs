using System;

namespace Valkyrie.Model
{
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