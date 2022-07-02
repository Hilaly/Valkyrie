using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Entities
{
    public abstract class TreeCollection<T>
    {
        private readonly List<TreeCollection<T>> _parents;
        private readonly List<T> _collection = new List<T>();

        protected TreeCollection(params TreeCollection<T>[] parents)
        {
            _parents = new List<TreeCollection<T>>(parents.Where(x => x != null));
        }

        public int Count => _collection.Count;
        public int TotalCount => Count + _parents.Sum(x => x.TotalCount);

        public List<TreeCollection<T>> GetParents() => _parents;
        public List<T> GetCollection(bool includeParent)
        {
            if (includeParent)
                return Enumerable.Empty<T>()
                    .Union(_parents.SelectMany(x => x.GetCollection(true)))
                    .Union(_collection)
                    .ToList();
            return _collection;
        }

        public void Add(T entity) => _collection.Add(entity);
        public void Remove(T entity) => _collection.Remove(entity);
    }
}