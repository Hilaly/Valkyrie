using UnityEngine;

namespace Valkyrie.Model
{
    public interface INode
    {
        string Uid { get; }
        
        string Name { get; }
        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }
    }
}