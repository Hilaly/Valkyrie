using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Model
{
    public interface INode
    {
        string Uid { get; }
        
        IGraph Graph { get; }
        
        string Name { get; set; }
        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }

        IPort GetPort(string name);
        IEnumerable<IPort> Ports { get; }

        public event Action<CemNodeChangedEvent> NodeChanged;

        void PrepareForDrawing();
    }
}