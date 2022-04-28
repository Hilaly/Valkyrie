using System;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsState
    {
        ref T Get<T>(EcsEntity e) where T : struct;
        void Add<T>(EcsEntity e, T component) where T : struct;
        void Remove<T>(EcsEntity e) where T : struct;
        bool Has<T>(EcsEntity e) where T : struct;
    }

    class EcsState : IEcsState
    {
        public class Data<T> where T : struct
        {
            public Pool<T> Pool = new Pool<T>();
            public ExistEcsFilter<T> ExistEcsFilter;
            public NotExistEcsFilter<T> NotExistEcsFilter;

            public Data(EcsState state)
            {
                ExistEcsFilter = new ExistEcsFilter<T>(state);
                NotExistEcsFilter = new NotExistEcsFilter<T>(state);
            }
        }

        private readonly Dictionary<Type, object> _data = new Dictionary<Type, object>();
        public event Action<int> OnEntityChanged;

        public Data<T> Get<T>() where T : struct
        {
            if (!_data.TryGetValue(typeof(T), out var result))
                _data.Add(typeof(T), result = new Data<T>(this));
            return (Data<T>)result;
        }

        public ref T Get<T>(EcsEntity e) where T : struct => ref Get<T>().Pool.GetById(e.Id);

        public void Add<T>(EcsEntity e, T component) where T : struct
        {
            if(Get<T>().Pool.AddById(e.Id, component))
                OnOnEntityChanged(e.Id);
        }

        public void Remove<T>(EcsEntity e) where T : struct
        {
            Get<T>().Pool.RemoveById(e.Id);
            OnOnEntityChanged(e.Id);
        }

        public bool Has<T>(EcsEntity e) where T : struct
        {
            Get<T>().Pool.GetById(e.Id, out var exist);
            return exist;
        }

        protected virtual void OnOnEntityChanged(int id)
        {
            OnEntityChanged?.Invoke(id);
        }
    }
}