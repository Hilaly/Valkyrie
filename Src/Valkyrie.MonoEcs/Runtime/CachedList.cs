using System.Collections;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public class CachedList<T> : IEnumerable<T>
    {
        private readonly List<T> _collection = new();
        private readonly List<T> _cache = new();
        private bool _needRebuildCache;

        void Rebuild()
        {
            if(!_needRebuildCache)
                return;
            
            _cache.Clear();
            _cache.AddRange(_collection);
            _needRebuildCache = false;
        }

        public List<T> Get()
        {
            Rebuild();
            return _cache;
        }

        public IEnumerator<T> GetEnumerator() => Get().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public void Add(T item)
        {
            _collection.Add(item);
            _needRebuildCache = true;
        }

        public void Remove(T item)
        {
            if (_collection.Remove(item))
                _needRebuildCache = true;
        }
    }
}