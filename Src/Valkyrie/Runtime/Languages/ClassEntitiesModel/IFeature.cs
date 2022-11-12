using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;

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
    {}
    
    public interface IFeature
    {
        public string Name { get; }
        
        void Import(WorldModelInfo world);
    }
    
    public interface IEntity { }
	
    public interface ISimSystem
    {
        void Simulate(float dt);
    }

    public interface IWorldLoader
    {
        void AddSystem(Valkyrie.ISimSystem simSystem);
        
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

    public abstract class BaseTypedSystem<T> : ISimSystem
        where T : IEntity
    {
        [field: Inject] protected IStateFilter<T> Filter { get; }

        public void Simulate(float dt) => Simulate(dt, Filter.GetAll());

        protected abstract void Simulate(float dt, IReadOnlyList<T> entities);
    }

    public interface IStateFilter<out T> where T : IEntity
    {
        IReadOnlyList<T> GetAll();
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
    }

    public class ResourcesViewsProvider : IViewsProvider
    {
        public void Release<TView>(TView value) where TView : Component => UnityEngine.Object.Destroy(value.gameObject);
        public TView Spawn<TView>(string prefabName) where TView : Component => UnityEngine.Object.Instantiate(Resources.Load<TView>(prefabName));
    }
	
    public class PoolViewsProvider : IViewsProvider
    {
        private readonly Dictionary<object, IDisposable> _cache = new();
        private readonly Valkyrie.Utils.Pool.IObjectsPool _objectsPool;
        public PoolViewsProvider(Valkyrie.Utils.Pool.IObjectsPool objectsPool)
        {
            _objectsPool = objectsPool;
        }
        void IViewsProvider.Release<TView>(TView value)
        {
            if(_cache.Remove(value, out var disposable)) disposable.Dispose();
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