using System.Collections;
using System.Collections.Generic;

namespace Valkyrie.Model
{
    public interface IGraph
    {
        IReadOnlyList<INode> Nodes { get; }
        string Name { get; }

        INode Create(INodeFactory nodeType);

        void Remove(INode node);
        void Connect(IPort output, IPort input);
        void Disconnect(IPort output, IPort input);
        void MarkDirty();

        IEnumerable<INodeFactory> GetFactories();
        
        IEnumerable<string> GetOutputConnections(string outputPortUid);
        IEnumerable<string> GetInputConnections(string inputPortUid);
        
        INode Clone(INode node);
    }
}