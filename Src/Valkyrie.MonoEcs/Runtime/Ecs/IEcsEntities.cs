using System;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsEntities
    {
        EcsEntity GetEntity(int id);
        EcsEntity CreateEntity();
        void Destroy(int id);
    }
    
    class EcsEntities : IEcsEntities
    {
        private int _idCounter = 1;
        private readonly HashSet<int> _entities = new HashSet<int>();
        private readonly EcsState _ecsState;

        public EcsEntities(EcsState ecsState)
        {
            _ecsState = ecsState;
        }

        public EcsEntity GetEntity(int id)
        {
            if(!_entities.Contains(id))
                throw new ArgumentOutOfRangeException($"Couldn't find entity {id}");
            return new EcsEntity() { Id = id };
        }

        public EcsEntity CreateEntity()
        {
            var id = _idCounter++;
            if (_entities.Add(id))
                return new EcsEntity() { Id = id };
            throw new Exception($"Couldn't create entity");
        }

        public void Destroy(int id)
        {
            _ecsState.Clear(new EcsEntity() { Id = id });
            if (!_entities.Remove(id))
                throw new Exception($"Entity {id} not exist");
        }

        public IEnumerable<int> GetAll() => _entities;
    }

}