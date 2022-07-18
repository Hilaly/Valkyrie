using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype.Mono
{
    public class HandleTriggers : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            var self = GetComponentInParent<EntityHolder>()?.Entity;
            var o = other.GetComponentInParent<EntityHolder>()?.Entity;

            if (self != null && o != null)
            {
                self.PropagateEvent(new TriggerEnterEvent() { Entity = o });
                o.PropagateEvent(new TriggerEnterEvent() { Entity = self });
            }
        }

        private void OnTriggerExit(Collider other)
        {
            var self = GetComponentInParent<EntityHolder>()?.Entity;
            var o = other.GetComponentInParent<EntityHolder>()?.Entity;

            if (self != null && o != null)
            {
                self.PropagateEvent(new TriggerExitEvent() { Entity = o });
                o.PropagateEvent(new TriggerExitEvent() { Entity = self });
            }
        }
    }
}