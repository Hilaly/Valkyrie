using System;
using UnityEngine;
using UnityEngine.Profiling;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public interface IFeature : ILibrary
    {
        void Install(IWorldController world);
    }

    public interface IComponent
    {
        IEntity Entity { get; }
    }

    public interface ISystem
    {
        void Simulate(float dt);
    }

    class ProfileSystem<T> : ISystem where T : ISystem
    {
        private T _instance;
        private readonly string _name = typeof(T).FullName;

        public ProfileSystem(T instance)
        {
            _instance = instance;
        }

        public void Simulate(float dt)
        {
            Profiler.BeginSample(_name);
            _instance.Simulate(dt);
            Profiler.EndSample();
        }
    }
}