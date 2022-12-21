using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Playground.Triggers
{
    public abstract class TriggerHandlerComponent : MonoComponent
    {
        [SerializeField] private List<Trigger3dBehaviour> triggers3d = new();

        IEnumerable<ITriggerBehaviour> GetBehaviours() => triggers3d;

        private void OnEnable()
        {
            foreach (var trigger in GetBehaviours())
            {
                trigger.TriggerEnter += OnTriggerEnter;
                trigger.TriggerExit += OnTriggerExit;
            }
        }

        private void OnDisable()
        {
            foreach (var trigger in GetBehaviours())
            {
                trigger.TriggerEnter -= OnTriggerEnter;
                trigger.TriggerExit -= OnTriggerExit;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponentInParent<EntityBehaviour>();
            if (e != null && IsValid(e))
                OnEnter(e);
        }

        private void OnTriggerExit(Collider other)
        {
            var e = other.GetComponentInParent<EntityBehaviour>();
            if (e != null && IsValid(e))
                OnExit(e);
        }

        protected virtual void OnExit(IEntity e) { }

        protected virtual void OnEnter(IEntity e) { }

        protected virtual bool IsValid(IEntity entity) => true;
    }
}