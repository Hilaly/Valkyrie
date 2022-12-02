using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie.Playground
{
    public abstract class Feature : IFeature
    {
        private IContainer _container;
        private IWorldController _world;

        private readonly List<Action> _registerCalls = new();
        private readonly List<Action> _installCalls = new();

        void ILibrary.Register(IContainer container)
        {
            _container = container;

            container.Register(this).AsInterfacesAndSelf();

            foreach (var registerCall in _registerCalls) registerCall();
        }

        void IFeature.Install(IWorldController world)
        {
            _world = world;
            foreach (var installCall in _installCalls) installCall();
            _world = null;
        }

        void Register<T>() where T : ISystem => _container.RegisterSingleInstance<T>();
        void Install<T>(int order) where T : ISystem => _world.RegisterSystem(_container.Resolve<T>(), order);

        protected void Register<T>(int order) where T : ISystem
        {
            _registerCalls.Add(Register<T>);
            _installCalls.Add(() => Install<T>(order));
        }
    }

    [RequireComponent(typeof(EntityBehaviour))]
    public abstract class MonoComponent : MonoBehaviour, IComponent
    {
        public IEntity Entity => gameObject.GetComponentInParent<EntityBehaviour>();
    }


    public abstract class BaseTypedSystem<T> : ISystem
        where T : IComponent
    {
        [Inject] private GameState _gameState;

        public void Simulate(float dt)
        {
            var list = _gameState.GetEntities().SelectMany(x => x.Get<T>()).ToList();
            Simulate(dt, list);
        }

        protected abstract void Simulate(float dt, IReadOnlyList<T> entities);
    }

    public abstract class BaseTypedSystem<T0, T1> : ISystem
        where T0 : IComponent
        where T1 : IComponent
    {
        [Inject] private GameState _gameState;

        public void Simulate(float dt)
        {
            var list = new List<Tuple<T0, T1>>();
            foreach (var entity in _gameState.GetEntities())
            {
                var t0 = entity.Get<T0>();
                if (t0.Count == 0)
                    continue;
                var t1 = entity.Get<T1>();
                if (t1.Count == 0)
                    continue;
                list.AddRange(from t0i in t0 from t1i in t1 select new Tuple<T0, T1>(t0i, t1i));
            }

            Simulate(dt, list);
        }

        protected abstract void Simulate(float dt, IReadOnlyList<Tuple<T0, T1>> entities);
    }

    public abstract class BaseTypedSystem<T0, T1, T2> : ISystem
        where T0 : IComponent
        where T1 : IComponent
        where T2 : IComponent
    {
        [Inject] private GameState _gameState;

        public void Simulate(float dt)
        {
            var list = new List<Tuple<T0, T1, T2>>();
            foreach (var entity in _gameState.GetEntities())
            {
                var t0 = entity.Get<T0>();
                if (t0.Count == 0)
                    continue;
                var t1 = entity.Get<T1>();
                if (t1.Count == 0)
                    continue;
                var t2 = entity.Get<T2>();
                if (t2.Count == 0)
                    continue;
                list.AddRange(from t0i in t0 from t1i in t1 from t2i in t2 select new Tuple<T0, T1, T2>(t0i, t1i, t2i));
            }

            Simulate(dt, list);
        }

        protected abstract void Simulate(float dt, IReadOnlyList<Tuple<T0, T1, T2>> entities);
    }
}