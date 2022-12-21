namespace Valkyrie.Playground.Triggers
{
    public abstract class FilterByExistComponentTriggerComponent<T> : TriggerHandlerComponent where T : IComponent
    {
        protected override bool IsValid(IEntity entityBehaviour)
        {
            return entityBehaviour.Get<T>() != null;
        }
    }
}