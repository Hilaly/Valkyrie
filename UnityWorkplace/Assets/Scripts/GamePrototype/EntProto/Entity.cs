using System;
using System.Collections.Generic;

namespace NaiveEntity.GamePrototype.EntProto
{
    public interface IComponent
    {
    }

    public class Entity : IEntity
    {
        private readonly Dictionary<string, List<IEntity>> _containers = new();

        public string Id { get; }
        public List<IComponent> Components { get; } = new List<IComponent>();

        public Entity(string id) => Id = id;

        #region Components

        public T GetComponent<T>() where T : IComponent => (T)Components.Find(x => x is T);

        public T AddComponent<T>(T component) where T : IComponent
        {
            if (GetComponent<T>() != null)
                throw new Exception($"Component {nameof(T)} already exist");
            Components.Add(component);
            return component;
        }

        public void RemoveComponent<T>() where T : IComponent => Components.RemoveAll(x => x is T);

        public void AddComponent(object c)
        {
            if (Components.Find(x => x.GetType() == c.GetType()) != null)
                throw new Exception($"Component {c.GetType().Name} already exist");
            Components.Add((IComponent)c);
        }

        #endregion

        #region Containers

        public IEnumerable<IEntity> GetContainer(string name) => _containers.TryGetValue(name, out var r) ? r : default;

        public void AddToContainer(string name, IEntity e)
        {
            if (!_containers.TryGetValue(name, out var c))
                _containers.Add(name, c = new List<IEntity>());

            c.Add(e);
        }

        public void RemoveFromContainer(string name, IEntity e)
        {
            if (!_containers.TryGetValue(name, out var c))
                return;

            c.Remove(e);
            if (c.Count == 0)
                _containers.Remove(name);
        }

        #endregion
    }
}