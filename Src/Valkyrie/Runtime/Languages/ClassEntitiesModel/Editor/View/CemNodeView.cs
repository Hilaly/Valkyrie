using System;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.UIElements;

namespace Valkyrie.View
{
    public interface INodeView
    {
        Rect GetPosition();
        void SetPosition(Rect pos);
        bool IsMovable { get; }
    }
    
    class CemNodeView : UnityEditor.Experimental.GraphView.Node, INodeView
    {
        public INode Node => (INode)userData;

        bool INodeView.IsMovable => true;

        public CemNodeView(INode node)
        {
            userData = node;
            viewDataKey = node.Uid;

            style.position = Position.Absolute;
            style.left = node.NodePosition.x;
            style.top = node.NodePosition.x;
            
            /*
            style.minWidth = node.NodeRect.size.x;
            style.minHeight = node.NodeRect.size.y;
            */
            
            title = node.Name;
        }
    }

    public interface INode
    {
        string Uid { get; }
        
        string Name { get; }
        Vector2 NodePosition { get; set; }
    }

    [Serializable]
    class CemNode : INode
    {
        [SerializeField, JsonProperty] private string uid = Guid.NewGuid().ToString();
        [SerializeField, JsonProperty] private Rect rect;
        
        public string Name { get; set; }
        
        [JsonIgnore] public string Uid => uid;
        [JsonIgnore] public Rect NodeRect
        {
            get => rect;
            set => rect = value;
        }
        [JsonIgnore] public Vector2 NodePosition
        {
            get => rect.position;
            set => rect.position = value;
        }
    }
}