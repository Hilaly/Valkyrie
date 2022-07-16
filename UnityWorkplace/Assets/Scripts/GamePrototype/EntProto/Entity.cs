using System;
using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto
{
    public class Entity : IEntity
    {
        private readonly List<object> _components = new List<object>();

        public string Id { get; }

        public List<object> Components => _components;

        public Entity(string id)
        {
            Id = id;
        }

        public T GetComponent<T>() => (T)_components.Find(x => x is T);

        public T AddComponent<T>(T component)
        {
            if (GetComponent<T>() != null)
                throw new Exception($"Component {nameof(T)} already exist");
            _components.Add(component);
            return component;
        }

        public void RemoveComponent<T>() => _components.RemoveAll(x => x is T);
    }
}