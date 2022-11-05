using UnityEngine;

namespace Valkyrie.View
{
    public interface INodeView
    {
        Rect GetPosition();
        void SetPosition(Rect pos);
        bool IsMovable { get; }
        
        Model.INode Node { get; }
    }
}