using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype.GameLogic
{
    public class BaseEvent
    {
        
    }

    public abstract class WithoutParametersEvent<T>
        : BaseEvent where T : new()
    {
        public static T Instance { get; } = new T();
    }

    public class UpdateEvent : BaseEvent
    {
        public readonly float DeltaTime;
        public readonly float Time;

        public UpdateEvent(float deltaTime, float time)
        {
            DeltaTime = deltaTime;
            Time = time;
        }
    }

    public class SpawnedEvent : WithoutParametersEvent<SpawnedEvent>
    {
        
    }

    public class InputChangedEvent : BaseEvent
    {
        public float DeltaTime;
        public Vector3 Input;
    }

    public class PositionChangedEvent : BaseEvent
    {
        public Vector3 PreviousPosition;
        public Vector3 Position;
    }

    public class TriggerEnterEvent : BaseEvent
    {
        public IEntity Entity;
    }
    public class TriggerExitEvent : BaseEvent
    {
        public IEntity Entity;
    }
}