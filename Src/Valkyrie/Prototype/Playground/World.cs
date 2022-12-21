using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.Ecs;

namespace Valkyrie.Playground
{
    public interface IArchetype
    {
        void Prepare(IEntity e);
    }

    public interface IWorldController
    {
        void RegisterSystem<T>(T inst, int order = 0) where T : ISystem;

        void Build();
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
            var temp = prefab.gameObject.activeSelf;
            prefab.gameObject.SetActive(false);
            var r = _container.Instantiate(prefab, position, rotation, transform);
            prefab.gameObject.SetActive(temp);
            r.gameObject.SetActive(true);
            return r;
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

        public void Build()
        {
            var allHandledTypes = new HashSet<Type>();
            foreach (var key in _systems.Keys)
                if (key is IEventCleaner cleaner)
                    allHandledTypes.UnionWith(cleaner.GetHandledTypes());

            var orderToCreate = _systems.Values.Max() + 1;
            var allEvents = typeof(IEventComponent).GetAllSubTypes(x => x.IsClass && !x.IsAbstract);
            foreach (var eventType in allEvents)
                if (!allHandledTypes.Contains(eventType))
                    _systems.Add(this.CreateEventClearSystem(eventType), orderToCreate);
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