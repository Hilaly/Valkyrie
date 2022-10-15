using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto
{
    public class EntityWrapper : IEntity
    {
        public IEntity Entity { get; set; }

        public EntityWrapper()
        {
        }

        public EntityWrapper(IEntity entity)
        {
            Entity = entity;
        }

        public string Id => Entity.Id;

        public T GetComponent<T>() where T : IComponent
        {
            return Entity.GetComponent<T>();
        }

        public T AddComponent<T>(T component) where T : IComponent
        {
            return Entity.AddComponent(component);
        }

        public void RemoveComponent<T>() where T : IComponent
        {
            Entity.RemoveComponent<T>();
        }

        public IEnumerable<IEntity> GetContainer(string name)
        {
            return Entity.GetContainer(name);
        }

        public void AddToContainer(string name, IEntity e)
        {
            Entity.AddToContainer(name, e);
        }

        public void RemoveFromContainer(string name, IEntity e)
        {
            Entity.RemoveFromContainer(name, e);
        }
    }
}