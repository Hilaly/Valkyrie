using System;

namespace Valkyrie.Profile
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TableAttribute : Attribute
    {
        public string Name { get; }

        public TableAttribute(string name)
        {
            Name = name;
        }
    }
}