using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.View
{
    public interface INodeView
    {
        VisualElement GetRoot();
        
        Rect GetPosition();
        void SetPosition(Rect pos);
        bool IsMovable { get; }
        
        Model.INode Node { get; }
    }
}