using UnityEngine;

namespace Valkyrie.Playground.Features
{
    public abstract class TriggerHandlerComponent : MonoComponent
    {
        private void OnTriggerEnter(Collider other)
        {
            var e = other.GetComponentInParent<EntityBehaviour>();
            if (e == null)
                return;

            if (IsValid(e))
                OnEnter(e);
        }

        private void OnTriggerExit(Collider other)
        {
            var e = other.GetComponentInParent<EntityBehaviour>();
            if (e == null)
                return;

            if (IsValid(e))
                OnExit(e);
        }

        protected virtual void OnExit(IEntity e) {}

        protected virtual void OnEnter(IEntity e) {}

        protected virtual bool IsValid(IEntity entity)
        {
            return true;
        }
    }

    public abstract class FilterByExistComponentTriggerComponent<T> : TriggerHandlerComponent where T : IComponent
    {
        protected override bool IsValid(IEntity entityBehaviour)
        {
            return entityBehaviour.Get<T>() != null;
        }
    }

    public abstract class PlayerEnterTriggerComponent : FilterByExistComponentTriggerComponent<IPlayerComponent>
    {
    }
}