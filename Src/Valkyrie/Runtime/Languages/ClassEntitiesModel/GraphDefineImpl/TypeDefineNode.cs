using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.GraphDefineImpl
{
    public abstract class GenericNodeFactory<T> : INodeFactory where T : INode, new()
    {
        public HashSet<string> Tags { get; set; } = new();
        public string Path { get; set; }
        public string Name { get; set; }
        public string Tooltip { get; set; }
        public bool Deletable { get; set; } = true;
        public bool Movable { get; set; } = true;
        public float MinWidth { get; set; } = 50;
        public float MinHeight { get; set; } = 10;
        
        public Vector2 MinSize => new Vector2(MinWidth, MinHeight);

        public GenericNodeFactory()
        {
            var type = typeof(T);
            
            Name = type.Name.Replace("Node", "").Replace(".", "/");
            Path = type.Namespace?.Replace(".", "/");
        }

        public INode Create()
        {
            return (INode)new T();
        }
    }

    public class TestNode : BaseNode
    {
        public class Factory : GenericNodeFactory<TestNode>
        {
            public Factory()
            {
                Tags = new HashSet<string> { "test" };
                Path = "Test";
                Tooltip = "Test Node";
            }
        }

        public override INodeFactory GetData()
        {
            return new Factory();
        }
    }
}