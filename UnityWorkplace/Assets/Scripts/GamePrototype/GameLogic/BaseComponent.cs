using System;
using GamePrototype.Mono;
using Hilaly.Utils;
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
            var go =
                entity.GetOrCreateComponent<ViewComponent>().Value =
                    Object.Instantiate(Resources.Load<GameObject>(Value),
                        entity.GetComponent<PositionComponent>().Value,
                        Quaternion.identity);

            var c = go.GetComponent<EntityHolder>() ?? go.AddComponent<EntityHolder>();
            c.Entity = entity;
        }
    }

    public class ReadKeyboardInputComponent : ValueComponent<Func<Vector2, Vector3>>, IEventConsumer<UpdateEvent>
    {
        public void PropagateEvent(IEntity entity, UpdateEvent e)
        {
            var h = Input.GetAxis("Horizontal");
            var v = Input.GetAxis("Vertical");

            entity.PropagateEvent(new InputChangedEvent()
            {
                Input = Value(new Vector2(h, v)),
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
    
    public class CameraFollowComponent : ValueComponent<CameraController>, IEventConsumer<PositionChangedEvent>
    {
        public void PropagateEvent(IEntity entity, PositionChangedEvent e)
        {
            Value.SetTarget(e.Position, Quaternion.identity);
        }
    }

    public class TownComponent : BaseComponent
    {
        
    }
}