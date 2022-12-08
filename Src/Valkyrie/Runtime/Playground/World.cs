using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Ecs;

namespace Valkyrie.Playground
{
    public interface IWorldController
    {
        void RegisterSystem<T>(T inst, int order = 0) where T : ISystem;
    }

    public interface IWorld
    {
        IEntity Create();
        IEntity Create(EntityBehaviour prefab, Vector3 position, Quaternion rotation);

        void Destroy(IEntity entity);
        void Destroy(Func<IEntity, bool> filter);
    }

    public class World : MonoBehaviour, IWorld, IWorldController
    {
        [Inject, SerializeField] private SimulationSettings _simulationSettings;

        [Inject] private IContainer _container;
        [Inject] private GameState _gameState;

        private readonly Dictionary<ISystem, int> _systems = new();

        public void RegisterSystem<T>(T inst, int order = 0) where T : ISystem
        {
            _systems.Add(new ProfileSystem<T>(inst, this), order);
        }

        public IEntity Create()
        {
            var go = new GameObject(Guid.NewGuid().ToString());
            go.SetActive(false);
            go.transform.parent = transform;
            var t = go.AddComponent<EntityBehaviour>();
            _container.InjectGameObject(go, true);
            go.SetActive(true);
            return t;
        }

        public IEntity Create(EntityBehaviour prefab, Vector3 position, Quaternion rotation)
        {
            return _container.Instantiate(prefab, position, rotation, transform);
        }

        public void Destroy(IEntity entity) =>
            (entity as IDisposable)?.Dispose();

        public void Destroy(Func<IEntity, bool> filter)
        {
            foreach (var e in _gameState.GetEntities())
                if (filter(e))
                    Destroy(e);
        }

        private void Update() => SimulateIteration();

        void SimulateIteration()
        {
            if (_simulationSettings.IsSimulationPaused) return;

            var dt = _simulationSettings.SimulationSpeed * Time.deltaTime;
            foreach (var (system, order) in _systems.OrderBy(x => x.Value))
                system.Simulate(dt);
        }
    }

    public class GameState
    {
        private readonly List<IEntity> _entities = new();

        internal IDisposable Register(IEntity entity)
        {
            Debug.LogWarning($"[TEST]: register {entity.Id} entity");
            _entities.Add(entity);
            return new ActionDisposable(() =>
            {
                Debug.LogWarning($"[TEST]: unregister {entity.Id} entity");
                _entities.Remove(entity);
            });
        }

        public IReadOnlyList<IEntity> GetEntities()
        {
            return new List<IEntity>(_entities);
        }
    }
}