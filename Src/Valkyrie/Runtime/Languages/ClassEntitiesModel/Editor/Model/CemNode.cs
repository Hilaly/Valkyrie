using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Valkyrie.Model
{
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