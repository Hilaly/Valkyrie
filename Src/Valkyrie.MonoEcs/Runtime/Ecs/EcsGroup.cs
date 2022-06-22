using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Ecs
{
    class EcsGroup : IEcsGroup, IDisposable
    {
        private readonly HashSet<EcsEntity> _entities = new HashSet<EcsEntity>();
        private readonly List<IEcsFilter> _ecsFilters;
        private readonly EcsState _state;

        public IEnumerator<EcsEntity> GetEnumerator() => _entities.ToList().GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int Count => _entities.Count;

        public EcsGroup(EcsState state, IEnumerable<IEcsFilter> filters)
        {
            _ecsFilters = new List<IEcsFilter>(filters);
            _state = state;
            _state.OnEntityChanged += OnEntityChanged;

            foreach (var e in _state.GetAll())
                OnEntityChanged(e);
        }

        public void Dispose()
        {
            _state.OnEntityChanged -= OnEntityChanged;
        }

        private void OnEntityChanged(int id)
        {
            var e = new EcsEntity { Id = id, State = _state };
            if (_ecsFilters.TrueForAll(x => x.IsMatch(e)))
                _entities.Add(e);
            else
                _entities.Remove(e);
        }

        public List<EcsEntity> GetEntities(List<EcsEntity> buffer)
        {
            buffer.Clear();
            buffer.AddRange(_entities);
            return buffer;
        }

        public List<int> GetEntities(List<int> buffer)
        {
            buffer.Clear();
            buffer.AddRange(_entities.Select(x => x.Id));
            return buffer;
        }
    }
}