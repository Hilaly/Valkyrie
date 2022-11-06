using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Model
{
    public interface INode
    {
        string Uid { get; }
        
        string Name { get; set; }
        Rect NodeRect { get; set; }
        Vector2 NodePosition { get; set; }
        
        IEnumerable<IPort> Ports { get; }

        public event Action<CemNodeChangedEvent> NodeChanged;
    }

    public interface INodeExt : INode
    {
        void OnCreate();
    }

    public interface INodeWithFields : INode
    {
        IEnumerable<INodeProperty> Properties { get; }
    }
    
    public interface IRenamable {}

    public interface INodeProperty
    {
        string Name { get; }
        Type PropertyType { get; }
        object Value { get; set; }
    }
}