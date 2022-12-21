using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valkyrie.Entities
{
    public interface IReadOnlyEntitiesContext
    {
        Entity GetEntity(string id);
        IReadOnlyList<Entity> GetEntities();
        
        IReadOnlyList<EntitiesContext> GetParents();
    }

    public class EntitiesContext : IReadOnlyEntitiesContext
    {
        private readonly List<EntitiesContext> _parents = new();
        private readonly List<Entity> _collection = new();

        public EntitiesContext(params EntitiesContext[] parents)
        {
            if(parents != null)
                _parents.AddRange(parents.Where(x => x != null));
        }

        public IReadOnlyList<EntitiesContext> GetParents() => _parents;

        public void Add(Entity entity) => _collection.Add(entity);
        public void Remove(Entity entity) => _collection.Remove(entity);

        public Entity GetEntity(string id) => _collection.Find(x => x.Id == id);

        public IReadOnlyList<Entity> GetEntities() => _collection;

        public void Destroy(Entity entity)
        {
            if (_collection.Remove(entity))
                entity.Dispose();
            else
                Debug.LogWarning($"[ENTITIES]: Try to destroy {entity.Id} not from owned context");
        }
    }
}