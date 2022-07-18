using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype.Mono
{
    public class EntityHolder : MonoBehaviour
    {
        public IEntity Entity { get; set; }
    }
}