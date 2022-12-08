using System;
using System.Collections.Generic;

namespace Valkyrie.Playground
{
    public static class EExtension
    {
        public static IReadOnlyList<T> GetAll<T>(this IComponent component) where T : IComponent =>
            component.Entity.GetAll<T>();

        public static T Get<T>(this IComponent component) where T : IComponent =>
            component.Entity.Get<T>();

        public static T SendEvent<T>(this GameState gameState, T eventInstance) where T : class, IEventComponent =>
            new EventEntity<T>(eventInstance, gameState).Get<T>();

        public static T SendEvent<T>(this GameState gameState) where T : class, IEventComponent, new() =>
            SendEvent(gameState, new T());

        internal static ISystem CreateEventClearSystem(this IWorld world, Type evType)
        {
            var systemType = typeof(EventClearSystem<>).MakeGenericType(evType);
            var systemInstance = Activator.CreateInstance(systemType, world);
            var result = (ISystem)Activator.CreateInstance(
                typeof(ProfileSystem<>).MakeGenericType(systemType),
                systemInstance, world);
            return result;
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

        public EventClearSystem(IWorld world)
        {
            _world = world;
        }

        public void Simulate(float dt)
        {
            _world.Destroy(x =>
            {
                var c = x.Get<T>();
                if (c != null)
                    UnityEngine.Debug.Log($"[TEST]: event {x.Id} CLEAR");
                return c != null;
            });
        }
    }
}