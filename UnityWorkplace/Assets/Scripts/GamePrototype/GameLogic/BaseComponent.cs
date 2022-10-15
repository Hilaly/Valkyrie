using System;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GamePrototype.GameLogic
{
    public abstract class BaseComponent : IComponent
    {
    }

    public class PositionComponent : ValueComponent<Vector3>
    {
    }

    public class RotationComponent : ValueComponent<Quaternion>
    {
    }

    public class ViewComponent : ValueComponent<GameObject>, IEventConsumer<PositionChangedEvent>
    {
        public void PropagateEvent(IEntity entity, PositionChangedEvent e)
        {
            Value.transform.position = e.Position;
        }
    }

    public class SpawnPrefabComponent : BaseComponent, IEventConsumer<SpawnedEvent>
    {
        public void PropagateEvent(IEntity entity, SpawnedEvent e)
        {
            var go =
                entity.GetOrCreateComponent<ViewComponent>().Value =
                    Object.Instantiate(Resources.Load<GameObject>(entity.GetComponent<PrefabComponent>()),
                        entity.GetComponent<PositionComponent>().Value,
                        entity.GetComponent<RotationComponent>()?.Value ?? Quaternion.identity);

            var c = go.GetComponent<EntityHolder>() ?? go.AddComponent<EntityHolder>();
            c.Entity = entity;
        }
    }

    public class PrefabComponent : ValueComponent<string>
    {
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

    public class BuildingComponent : BaseComponent
    {
    }

    public class PlayerEnterTriggerComponent : BaseComponent
        , IEventConsumer<TriggerEnterEvent>
        , IEventConsumer<TriggerExitEvent>
    {
        public void PropagateEvent(IEntity entity, TriggerEnterEvent e)
        {
            Debug.Log($"ENTER TRIGGER {e.Entity.Id}");
        }

        public void PropagateEvent(IEntity entity, TriggerExitEvent e)
        {
            Debug.Log($"EXIT TRIGGER {e.Entity.Id}");
        }
    }
}