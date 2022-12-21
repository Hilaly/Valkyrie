using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public interface IFeature : ILibrary
    {
        void Install(IWorldController world);
    }

    /// <summary>
    /// Base component interface
    /// </summary>
    public interface IComponent
    {
        IEntity Entity { get; }
    }

    public interface ITypedComponent<T> : IComponent
    {
        public T Value { get; set; }
    }

    public abstract class TypedComponent<T> : MonoComponent, ITypedComponent<T>
    {
        [SerializeField] private T _value;

        public T Value
        {
            get => _value;
            set => _value = value;
        }
    }
    
    /// <summary>
    /// Base event component
    /// </summary>
    public interface IEventComponent : IComponent
    {}

    /// <summary>
    /// Base system interface
    /// </summary>
    public interface ISystem
    {
        void Simulate(float dt);
    }

    /// <summary>
    /// Mark system as clear events system (they will be cleared before this system)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IEventClearSystem<T> where T : IEventComponent {}
    
    /// <summary>
    /// Mark system as consumer of events (they will be cleared after this system)
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IRequestConsumeSystem<T> where T : IEventComponent {}

    interface IEventCleaner
    {
        IEnumerable<Type> GetHandledTypes();
    }
    
    class ProfileSystem<T> : ISystem, IEventCleaner 
        where T : ISystem
    {
        private readonly T _instance;
        private readonly string _name = typeof(T).FullName;
        private readonly List<ISystem> _preSystems = new();
        private readonly List<ISystem> _postSystems = new();

        public IEnumerable<Type> GetHandledTypes()
        {
            var interfaces = _instance.GetType().GetInterfaces();
            foreach (var type in interfaces)
            {
                if (IsImplementGenericInterface(type, typeof(IEventClearSystem<>)))
                    yield return type.GetGenericArguments()[0];
                if (IsImplementGenericInterface(type, typeof(IRequestConsumeSystem<>)))
                    yield return type.GetGenericArguments()[0];
            }
        }

        public ProfileSystem(T instance, IWorld world)
        {
            _instance = instance;

            var interfaces = _instance.GetType().GetInterfaces();
            foreach (var @type in interfaces)
            {
                if (IsImplementGenericInterface(type, typeof(IEventClearSystem<>)))
                {
                    var eventType = type.GetGenericArguments()[0];
                    _preSystems.Add(world.CreateEventClearSystem(eventType));
                }
                if (IsImplementGenericInterface(type, typeof(IRequestConsumeSystem<>)))
                {
                    var eventType = type.GetGenericArguments()[0];

                    _postSystems.Add(world.CreateEventClearSystem(eventType));
                }
            }
        }

        bool IsImplementGenericInterface(Type type, Type genericInterface) =>
            type.IsConstructedGenericType && type.GetGenericTypeDefinition() == genericInterface;

        public void Simulate(float dt)
        {
            Profiler.BeginSample(_name);

            for (var i = 0; i < _preSystems.Count; ++i)
                _preSystems[i].Simulate(dt);

            _instance.Simulate(dt);

            for (var i = 0; i < _postSystems.Count; ++i)
                _postSystems[i].Simulate(dt);

            Profiler.EndSample();
        }
    }

    class EventEntity<TEvent> : IEntity, IDisposable
        where TEvent : class, IEventComponent
    {
        public readonly TEvent EventComponent;
        private IDisposable _disposable;

        public void Dispose()
        {
            _disposable?.Dispose();
            _disposable = null;
        }

        public EventEntity(TEvent eventComponent, GameState gameState)
        {
            EventComponent = eventComponent;
            _disposable = gameState.Register(this);
        }

        #region IEntity

        public string Id { get; } = Guid.NewGuid().ToString();

        public T Get<T>() where T : IComponent =>
            EventComponent is T result
                ? result
                : default;

        public IReadOnlyList<T> GetAll<T>() where T : IComponent =>
            EventComponent is T result
                ? new[] { result }
                : Array.Empty<T>();

        public T Add<T>() where T : MonoComponent
        {
            throw new System.NotImplementedException("Adding components to event entity is not implemented");
        }

        #endregion
    }

    class EventClearSystem<T> : ISystem
        where T : IEventComponent
    {
        private readonly IWorld _world;

        public EventClearSystem(IWorld world) => _world = world;

        public void Simulate(float dt) => _world.Destroy(x => x.Get<T>() != null);
    }
}