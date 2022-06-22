using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Ecs
{
    class GroupBuilder : IGroupBuilder
    {
        private readonly Dictionary<string, EcsGroup> _groups;
        private readonly List<IEcsFilter> _filters = new List<IEcsFilter>();
        private readonly EcsState _entities;
        private readonly EcsState _state;

        public GroupBuilder(Dictionary<string, EcsGroup> groups, EcsState state, EcsState entities)
        {
            _groups = groups;
            _state = state;
            _entities = entities;
        }

        public IGroupBuilder AllOf<T0, T1, T2, T3, T4>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            => AllOf<T0>().AllOf<T1>().AllOf<T2>().AllOf<T3>().AllOf<T4>();

        public IGroupBuilder AllOf<T0, T1, T2, T3>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            => AllOf<T0>().AllOf<T1>().AllOf<T2>().AllOf<T3>();

        public IGroupBuilder AllOf<T0, T1, T2>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            => AllOf<T0>().AllOf<T1>().AllOf<T2>();

        public IGroupBuilder AllOf<T0, T1>()
            where T0 : struct
            where T1 : struct
            => AllOf<T0>().AllOf<T1>();

        public IGroupBuilder AllOf<T0>() where T0 : struct
        {
            _filters.Add(_state.Get<T0>().ExistEcsFilter);
            return this;
        }

        public IGroupBuilder NotOf<T0, T1, T2, T3, T4>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
            => NotOf<T0>().NotOf<T1>().NotOf<T2>().NotOf<T3>().NotOf<T4>();

        public IGroupBuilder NotOf<T0, T1, T2, T3>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            => NotOf<T0>().NotOf<T1>().NotOf<T2>().NotOf<T3>();

        public IGroupBuilder NotOf<T0, T1, T2>()
            where T0 : struct
            where T1 : struct
            where T2 : struct
            => NotOf<T0>().NotOf<T1>().NotOf<T2>();

        public IGroupBuilder NotOf<T0, T1>()
            where T0 : struct
            where T1 : struct
            => NotOf<T0>().NotOf<T1>();

        public IGroupBuilder NotOf<T0>()
            where T0 : struct
        {
            _filters.Add(_state.Get<T0>().NotExistEcsFilter);
            return this;
        }

        public IGroupBuilder AnyOf<T0, T1, T2, T3, T4>() where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
            where T4 : struct
        {
            _filters.Add(new AnyOfEcsFilter(
                _state.Get<T0>().ExistEcsFilter,
                _state.Get<T1>().ExistEcsFilter,
                _state.Get<T2>().ExistEcsFilter,
                _state.Get<T3>().ExistEcsFilter,
                _state.Get<T4>().ExistEcsFilter
            ));
            return this;
        }

        public IGroupBuilder AnyOf<T0, T1, T2, T3>() where T0 : struct
            where T1 : struct
            where T2 : struct
            where T3 : struct
        {
            _filters.Add(new AnyOfEcsFilter(
                _state.Get<T0>().ExistEcsFilter,
                _state.Get<T1>().ExistEcsFilter,
                _state.Get<T2>().ExistEcsFilter,
                _state.Get<T3>().ExistEcsFilter
            ));
            return this;
        }

        public IGroupBuilder AnyOf<T0, T1, T2>() where T0 : struct where T1 : struct where T2 : struct
        {
            _filters.Add(new AnyOfEcsFilter(
                _state.Get<T0>().ExistEcsFilter,
                _state.Get<T1>().ExistEcsFilter,
                _state.Get<T2>().ExistEcsFilter
            ));
            return this;
        }

        public IGroupBuilder AnyOf<T0, T1>() where T0 : struct where T1 : struct
        {
            _filters.Add(new AnyOfEcsFilter(
                _state.Get<T0>().ExistEcsFilter,
                _state.Get<T1>().ExistEcsFilter
            ));
            return this;
        }

        public IEcsGroup Build()
        {
            var list = _filters.OrderBy(x => x.GetHash()).ToList();
            var hash = string.Join('&', list.Select(x => x.GetHash()));
            if (!_groups.TryGetValue(hash, out var result))
                _groups.Add(hash, result = new EcsGroup(_state, list));
            return result;
        }
    }
}