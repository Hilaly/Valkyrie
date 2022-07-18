namespace NaiveEntity.GamePrototype.EntProto.PoolProto
{
    public class SimpleClassPool<T> : Pool<T> where T : new()
    {
        public SimpleClassPool() : base(() => new T(), _ => {})
        {}
    }
}