using UnityEngine;
using Valkyrie.Di;

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

    public abstract class BasePlayerTriggerEvent : IEventComponent
    {
        public IEntity Entity { get; }

        public IEntity TriggerEntity;
        public IEntity PlayerEntity;
    }

    public class PlayerEnterTriggerEvent : BasePlayerTriggerEvent
    {
    }

    public class PlayerExitTriggerEvent : BasePlayerTriggerEvent
    {
    }

    public class PlayerTriggerComponent : FilterByExistComponentTriggerComponent<IPlayerComponent>
    {
        [Inject] private GameState _world;
        
        protected override void OnEnter(IEntity e)
        {
            _world.SendEvent(new PlayerEnterTriggerEvent { PlayerEntity = e, TriggerEntity = Entity });
        }

        protected override void OnExit(IEntity e)
        {
            _world.SendEvent(new PlayerExitTriggerEvent { PlayerEntity = e, TriggerEntity = Entity });
        }
    }
}