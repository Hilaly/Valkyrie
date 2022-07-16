namespace NaiveEntity.GamePrototype.EntProto
{
    public interface IEntity
    {
        public string Id { get; }

        T GetComponent<T>();
        T AddComponent<T>(T component);
        void RemoveComponent<T>();
    }
}