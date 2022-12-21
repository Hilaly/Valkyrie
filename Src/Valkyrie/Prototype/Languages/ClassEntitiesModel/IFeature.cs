using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Ecs;

namespace Valkyrie
{
    public enum ViewsSpawnType
    {
        Custom,
        Resources,
        Pool
    }

    public enum SimulationType
    {
        None,
        Fixed,
        Floating
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredPropertyAttribute : Attribute
    {
    }

    public interface IFeature
    {
        public string Name { get; }

        void Import(WorldModelInfo world);
    }

    public class GroupConverter<T, TU>
        where T : class, IEntityWrapper, TU, new()
    {
        private readonly GroupWrapper _wrapper;
        private readonly List<T> _output = new();

        public GroupConverter(IEcsGroup @group) => _wrapper = GroupWrapper.Wrap(@group);

        public List<EcsEntity> AsEntities() => _wrapper.Entities;

        public IReadOnlyList<TU> AsConverted()
        {
            var input = AsEntities();
            var output = _output;
            for (var index = 0; index < input.Count; ++index)
            {
                if (output.Count > index)
                    output[index].Entity = input[index];
                else
                    output.Add(new T { Entity = input[index] });
            }

            if (output.Count > input.Count)
            {
                output.RemoveRange(input.Count, output.Count - input.Count);
            }

            return output;
        }
    }

    public interface IEntityWrapper
    {
        public EcsEntity Entity { get; set; }
    }

    public interface IEntity
    {
    }

    public interface IExtEntity : IEntity
    {
        int Id { get; }
        
        bool IsDestroyed { get; }
        
        void Destroy();
    }

    public interface IEventEntity : IEntity
    {
    }

    public interface ISharedSystem
    {
    }

    public interface IEventSystem<T> : ISharedSystem where T : IEventEntity
    {
    }

    public interface IArchetypeSimSystem<in T> : ISharedSystem
        where T : IEntity
    {
        void Simulate(IReadOnlyList<T> e, float dt);
    }

    public interface IArchetypeEntitySimSystem<in T> : ISharedSystem
        where T : IEntity
    {
        void Simulate(T e, float dt);
    }

    public interface ISimSystem : ISharedSystem
    {
        void Simulate(float dt);
    }

    public interface IWorldLoader
    {
        void AddSystem<T>(T simSystem, int order = SimulationOrder.Default) where T : ISimSystem;

        Task InstallSystems();
    }

    public abstract class BaseEntitySimulateSystem<T> : BaseTypedSystem<T> where T : IEntity
    {
        protected override void Simulate(float dt, IReadOnlyList<T> entities)
        {
            for (var index = 0; index < entities.Count; index++)
                Simulate(entities[index], dt);
        }

        protected abstract void Simulate(T entity, float dt);
    }

    public class ProfileSystem<T> : ISimSystem
        where T : ISimSystem
    {
        private readonly T _innerSystem;
        private readonly string _sectionName;

        public ProfileSystem(T innerSystem)
        {
            _innerSystem = innerSystem;
            _sectionName = typeof(T).FullName;
        }

        public void Simulate(float dt)
        {
            UnityEngine.Profiling.Profiler.BeginSample(_sectionName);
            _innerSystem.Simulate(dt);
            UnityEngine.Profiling.Profiler.EndSample();
        }
    }

    public abstract class BaseTypedSystem<T> : ISimSystem
        where T : IEntity
    {
        [field: Inject] protected IWorldFilter<T> Filter { get; }

        public void Simulate(float dt) => Simulate(dt, Filter.GetAll());

        protected abstract void Simulate(float dt, IReadOnlyList<T> entities);
    }

    public abstract class BaseFilteredSystem<T> : BaseTypedSystem<T> 
        where T : IEntity
    {
        protected override void Simulate(float dt, IReadOnlyList<T> entities)
        {
            foreach (var entity in entities)
            {
                if(!IsFit(entity, dt))
                    continue;
                Simulate(dt, entity);
            }
        }

        protected abstract void Simulate(float dt, T entity);

        protected abstract bool IsFit(T entity, float dt);
    }

    public interface IWorldFilter<out T> where T : IEntity
    {
        IReadOnlyList<T> GetAll();
    }

    public interface IWorldCreator<out T> where T : IEntity
    {
        T Create();
    }

    public interface ITimer
    {
        float FullTime { get; }
        float TimeLeft { get; }
    }

    public interface IView<in TModel>
    {
        void UpdateDate(TModel model);
    }

    public interface IViewsProvider
    {
        void Release<TView>(TView value) where TView : Component;
        TView Spawn<TView>(string prefabName) where TView : Component;
        TView Spawn<TView>(string prefabName, Vector3 position, Quaternion rotation) where TView : Component;
    }

    public class ResourcesViewsProvider : IViewsProvider
    {
        public void Release<TView>(TView value) where TView : Component => UnityEngine.Object.Destroy(value.gameObject);

        public TView Spawn<TView>(string prefabName) where TView : Component =>
            UnityEngine.Object.Instantiate(Resources.Load<TView>(prefabName));

        public TView Spawn<TView>(string prefabName, Vector3 position, Quaternion rotation) where TView : Component =>
            UnityEngine.Object.Instantiate(Resources.Load<TView>(prefabName), position, rotation);
    }

    public class PoolViewsProvider : IViewsProvider
    {
        private readonly Dictionary<object, IDisposable> _cache = new();
        private readonly Utils.Pool.IObjectsPool _objectsPool;

        public PoolViewsProvider(Utils.Pool.IObjectsPool objectsPool)
        {
            _objectsPool = objectsPool;
        }

        void IViewsProvider.Release<TView>(TView value)
        {
            if (_cache.Remove(value, out var disposable)) disposable.Dispose();
        }

        TView IViewsProvider.Spawn<TView>(string prefabName)
        {
            var disposable = _objectsPool.Instantiate<TView>(prefabName);
#if UNITY_EDITOR
            Debug.Assert(disposable.Instance != null, $"Couldn't find {typeof(TView).Name} component on {prefabName}");
#endif
            _cache.Add(disposable.Instance, disposable);
            return disposable.Instance;
        }

        public TView Spawn<TView>(string prefabName, Vector3 position, Quaternion rotation) where TView : Component
        {
            var disposable = _objectsPool.Instantiate<TView>(prefabName, position, rotation);
#if UNITY_EDITOR
            Debug.Assert(disposable.Instance != null, $"Couldn't find {typeof(TView).Name} component on {prefabName}");
#endif
            _cache.Add(disposable.Instance, disposable);
            return disposable.Instance;
        }
    }

    public class EntityTimer : ITimer
    {
        public float FullTime { get; }
        public float TimeLeft { get; private set; }

        public EntityTimer(float time)
        {
            FullTime = TimeLeft = time;
        }

        public void Advance(float dt) => TimeLeft -= dt;
    }
}