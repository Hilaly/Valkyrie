using System;
using System.Collections.Generic;
using System.Linq;
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

    class ProfileSystem<T> : ISystem where T : ISystem
    {
        private readonly T _instance;
        private readonly string _name = typeof(T).FullName;
        private readonly List<ISystem> _preSystems = new();
        private readonly List<ISystem> _postSystems = new();

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
}