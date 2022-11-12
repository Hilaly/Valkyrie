using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
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

    public abstract class BaseTypedSystem<T> : ISimSystem
        where T : IEntity
    {
        [Inject] private IStateFilter<T> _stateFilter;
        
        public virtual void Simulate(float dt)
        {
            foreach (var e in _stateFilter.GetAll())
                Simulate(e, dt);
        }

        protected abstract void Simulate(T entity, float dt);
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