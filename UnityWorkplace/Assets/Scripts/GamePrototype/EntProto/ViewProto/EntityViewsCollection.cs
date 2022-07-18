using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NaiveEntity.GamePrototype.EntProto.PoolProto;
using Tools;

namespace NaiveEntity.GamePrototype.EntProto.ViewProto
{
    public class EntityViewsCollection<T> : IEnumerable<T> where T : EntityView, new()
    {
        private readonly EntityContext _entityContext;
        private readonly Func<IEntity, bool> _filter;
        private readonly SimpleClassPool<T> _pool = new SimpleClassPool<T>();
        private readonly List<T> _instances = new List<T>();

        //TODO: replace with filterGroup
        public EntityViewsCollection(EntityContext entityContext, Func<IEntity, bool> filter)
        {
            _entityContext = entityContext;
            _filter = filter;
        }

        public IEnumerator<T> GetEnumerator()
        {
            BuildCache();
            return _instances.GetEnumerator();
        }

        private void BuildCache()
        {
            var sourceEntities = _entityContext.Get().Where(x => _filter(x)).ToList();
            sourceEntities.SyncToViewDeleteExtra(
                _instances,
                x => _pool.Get(),
                (entity, view) => view.Entity = entity,
                view => _pool.Store(view));
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}