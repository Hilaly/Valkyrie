using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NaiveEntity.GamePrototype.EntProto
{
    public class EntityContext
    {
        private readonly Dictionary<string, Entity> _entities = new Dictionary<string, Entity>();

        public IEntity Get(string id) => _entities.TryGetValue(id, out var r) ? r : default(IEntity);

        public List<IEntity> Get() => _entities.Values.OfType<IEntity>().ToList();

        public IEntity Create(string id)
        {
            if (_entities.ContainsKey(id))
                throw new Exception($"Entity {id} already exist");
            var e = new Entity(id);
            _entities.Add(id, e);
            Debug.Log($"[ECS]: create entity {id}");
            return e;
        }
    }
}