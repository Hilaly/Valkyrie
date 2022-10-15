namespace NaiveEntity.GamePrototype.EntProto
{
    public interface IEventConsumer<in T> where T : class
    {
        void PropagateEvent(IEntity entity, T e);
    }
}