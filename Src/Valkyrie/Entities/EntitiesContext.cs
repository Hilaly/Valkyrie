using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Entities
{
    public class EntitiesContext : TreeCollection<Entity>
    {
        public EntitiesContext(EntitiesContext parent) : base(parent)
        {
        }

        public Entity GetEntity(string id, bool includeParent = false) =>
            GetEntities(includeParent).Find(x => x.Id == id);

        public List<Entity> GetEntities(bool includeParent = false) => base.GetCollection(includeParent);

        public void Destroy(Entity entity)
        {
            if (base.GetCollection(false).Remove(entity))
                entity.Dispose();
            else
                Debug.LogWarning($"[ENTITIES]: Try to destroy {entity.Id} not from owned context");
        }
    }
}