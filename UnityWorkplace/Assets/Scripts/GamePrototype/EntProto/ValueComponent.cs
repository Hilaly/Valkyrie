namespace NaiveEntity.GamePrototype.EntProto
{
    public abstract class ValueComponent<T> : IComponent
    {
        public T Value;
        public static implicit operator T(ValueComponent<T> d) => d.Value;
    }
    
    public sealed class IdComponent : ValueComponent<string>
    {}
}