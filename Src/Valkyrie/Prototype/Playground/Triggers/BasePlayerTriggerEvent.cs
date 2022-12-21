namespace Valkyrie.Playground.Triggers
{
    public abstract class BasePlayerTriggerEvent : IEventComponent
    {
        public IEntity Entity { get; }

        public IEntity TriggerEntity;
        public IEntity PlayerEntity;
    }
}