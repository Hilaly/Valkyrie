using System;

namespace Valkyrie.Model
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class DependsOnProperty : Attribute
    {
        public string PropertyName { get; }

        public DependsOnProperty(string propertyName)
        {
            PropertyName = propertyName;
        }
    }
}