using System.Collections.Generic;
using Meta;

namespace NaiveEntity.GamePrototype.EntProto
{
    [IsValidBindingType]
    public interface IEntity
    {
        public string Id { get; }

        T GetComponent<T>() where T : IComponent;
        T AddComponent<T>(T component) where T : IComponent;
        void RemoveComponent<T>() where T : IComponent;

        IEnumerable<IEntity> GetContainer(string name);
        void AddToContainer(string name, IEntity e);
        void RemoveFromContainer(string name, IEntity e);
    }
}