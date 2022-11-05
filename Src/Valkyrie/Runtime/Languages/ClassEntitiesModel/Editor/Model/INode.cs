using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Model
{
    public interface INode
    {
        string Uid { get; }
        
        string Name { get; }
        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }
        
        IEnumerable<IPort> Ports { get; }
    }

    public interface INodeExt : INode
    {
        void OnCreate();
    }
}