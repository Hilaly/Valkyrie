using System;

namespace Valkyrie.Model
{
    public interface INodeProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object Value { get; set; }
    }
}