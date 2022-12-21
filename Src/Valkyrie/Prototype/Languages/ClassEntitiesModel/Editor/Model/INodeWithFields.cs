using System.Collections.Generic;

namespace Valkyrie.Model
{
    public interface INodeWithFields : INode
    {
        IEnumerable<INodeProperty> Properties { get; }
    }
}