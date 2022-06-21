using System;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    class EcsState : IEcsState
    {
        private int _idCounter = 1;
        private readonly HashSet<int> _entities = new HashSet<int>();

        public EcsEntity GetEntity(int id)
        {
            if(!_entities.Contains(id))
                throw new ArgumentOutOfRangeException($"Couldn't find entity {id}");
            return new EcsEntity() { Id = id, State = this };
        }

        public EcsEntity CreateEntity()
        {
            return new EcsEntity() { Id = Generate(), State = this };
        }

        public int Generate()
        {
            var id = _idCounter++;
            if (_entities.Add(id))
                return id;
            throw new Exception($"Couldn't create entity");
        }

        public void Destroy(int id)
        {
            Clear(new EcsEntity() { Id = id, State = this });
            if (!_entities.Remove(id))
                throw new Exception($"Entity {id} not exist");
        }

        public IEnumerable<int> GetAll() => _entities;
    
        public interface IData
        {
            IPool GetPool();
        }

        public class Data<T> : IData where T : struct
        {
            public Pool<T> Pool = new Pool<T>();
            public ExistEcsFilter<T> ExistEcsFilter;
            public NotExistEcsFilter<T> NotExistEcsFilter;

            IPool IData.GetPool() => this.Pool;

            public Data(EcsState state)
            {
                ExistEcsFilter = new ExistEcsFilter<T>(state);
                NotExistEcsFilter = new NotExistEcsFilter<T>(state);
            }
        }

        private readonly Dictionary<Type, IData> _data = new Dictionary<Type, IData>();
        public event Action<int> OnEntityChanged;

        public Data<T> Get<T>() where T : struct
        {
            if (!_data.TryGetValue(typeof(T), out var result))
                _data.Add(typeof(T), result = new Data<T>(this));
            return (Data<T>)result;
        }

        public ref T Get<T>(EcsEntity e) where T : struct => ref Get<T>(e.Id);

        public void Add<T>(EcsEntity e, T component) where T : struct
            => Add(e.Id, component);

        public ref T Get<T>(int eId) where T : struct => ref Get<T>().Pool.GetById(eId);

        public void Add<T>(int eId, T component) where T : struct
        {
            if(Get<T>().Pool.AddById(eId, component))
                OnOnEntityChanged(eId);
        }

        public bool Has<T>(int eId) where T : struct
        {
            Get<T>().Pool.GetById(eId, out var exist);
            return exist;
        }

        public void Remove<T>(EcsEntity e) where T : struct
        {
            if(Get<T>().Pool.RemoveById(e.Id))
                OnOnEntityChanged(e.Id);
        }

        public bool Has<T>(EcsEntity e) where T : struct => Has<T>(e.Id);

        public void Clear(EcsEntity e)
        {
            var any = false;
            foreach (var pair in _data)
                if (pair.Value.GetPool().RemoveById(e.Id))
                    any = true;
            if(any)
                OnOnEntityChanged(e.Id);
        }

        protected virtual void OnOnEntityChanged(int id)
        {
            OnEntityChanged?.Invoke(id);
        }
    }
}