using System;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePrototype.GameLogic
{
    public abstract class BaseComponent
    {
    }

    public abstract class ValueComponent<T> : BaseComponent
    {
        public T Value;
    }

    public class PositionComponent : ValueComponent<Vector3>
    {
    }

    public class ViewComponent : ValueComponent<GameObject>, IEventConsumer<PositionChangedEvent>
    {
        public void PropagateEvent(IEntity entity, PositionChangedEvent e)
        {
            Value.transform.position = e.Position;
        }
    }

    public class PrefabComponent : ValueComponent<string>, IEventConsumer<SpawnedEvent>
    {
        public void PropagateEvent(IEntity entity, SpawnedEvent e)
        {
            entity.GetOrCreateComponent<ViewComponent>().Value =
                Object.Instantiate(Resources.Load<GameObject>(Value),
                    entity.GetComponent<PositionComponent>().Value,
                    Quaternion.identity);
        }
    }

    public class ReadKeyboardInputComponent : BaseComponent, IEventConsumer<UpdateEvent>
    {
        public Func<Vector2, Vector3> InputConverter;

        public void PropagateEvent(IEntity entity, UpdateEvent e)
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            entity.PropagateEvent(new InputChangedEvent()
            {
                Input = InputConverter(new Vector2(h, v)),
                DeltaTime = e.DeltaTime
            });
        }
    }

    public class MoveCapabilityComponent : BaseComponent, IEventConsumer<InputChangedEvent>
    {
        public void PropagateEvent(IEntity entity, InputChangedEvent e)
        {
            var ev = new PositionChangedEvent
            {
                PreviousPosition = entity.GetOrCreateComponent<PositionComponent>().Value,
                Position = entity.GetOrCreateComponent<PositionComponent>().Value += e.Input * e.DeltaTime
            };
            entity.PropagateEvent(ev);
        }
    }
}