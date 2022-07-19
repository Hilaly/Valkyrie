using System;
using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto
{
    public interface IComponent
    {}
    public class Entity : IEntity
    {
        private readonly List<IComponent> _components = new List<IComponent>();

        public string Id { get; }

        public List<IComponent> Components => _components;

        public Entity(string id)
        {
            Id = id;
        }

        public T GetComponent<T>() where T : IComponent => (T)_components.Find(x => x is T);

        public T AddComponent<T>(T component) where T : IComponent
        {
            if (GetComponent<T>() != null)
                throw new Exception($"Component {nameof(T)} already exist");
            _components.Add(component);
            return component;
        }

        public void RemoveComponent<T>() where T : IComponent => _components.RemoveAll(x => x is T);

        public void AddComponent(object c)
        {
            if (_components.Find(x => x.GetType() == c.GetType()) != null)
                throw new Exception($"Component {c.GetType().Name} already exist");
            _components.Add((IComponent)c);
        }
    }
}