using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Tools;

namespace Valkyrie
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
        
        public Vector2 MinSize => new(MinWidth, MinHeight);

        protected GenericNodeFactory()
        {
            var type = typeof(T);
            var attr = NodeAttribute.Cache[type];
            if (attr != null)
            {
                Tags = attr.Tags;
                Path = attr.Path;
                Name = attr.Name;
                Tooltip = attr.Tooltip;
                Deletable = attr.Deletable;
                Movable = attr.Movable;
                MinWidth = attr.MinWidth;
                MinHeight = attr.MinHeight;
            }
            if(Name.IsNullOrEmpty())
                Name = type.Name.Replace("Node", "").Replace(".", "/");
            if(Path.IsNullOrEmpty())
                Path = type.Namespace?.Replace(".", "/");
        }

        public INode Create()
        {
            var r = (INode)new T();
            r.Define();
            return r;
        }
    }
}