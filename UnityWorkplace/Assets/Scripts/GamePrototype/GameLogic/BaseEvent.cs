namespace GamePrototype.GameLogic
{
    public class BaseEvent
    {
        
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
}