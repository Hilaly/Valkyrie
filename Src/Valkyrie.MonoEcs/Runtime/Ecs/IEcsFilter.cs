using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Ecs
{
    internal interface IEcsFilter
    {
        bool IsMatch(EcsEntity e);
        string GetHash();
    }

    class AnyOfEcsFilter : IEcsFilter
    {
        private readonly List<IEcsFilter> _ecsFilters;

        public AnyOfEcsFilter(IEnumerable<IEcsFilter> ecsFilters)
        {
            _ecsFilters = new List<IEcsFilter>(ecsFilters);
        }

        public bool IsMatch(EcsEntity e)
        {
            return _ecsFilters.Any(x => x.IsMatch(e));
        }

        public string GetHash() => string.Join("|", _ecsFilters.Select(x => x.GetHash()));
    }
    
    class ExistEcsFilter<T> : IEcsFilter where T : struct
    {
        private readonly EcsState _state;

        public ExistEcsFilter(EcsState state)
        {
            _state = state;
        }

        public bool IsMatch(EcsEntity e)
        {
            return _state.Has<T>(e);
        }

        public string GetHash() => $"EX<{typeof(T).FullName}>";
    }

    class NotExistEcsFilter<T> : IEcsFilter where T : struct
    {
        private readonly EcsState _state;

        public NotExistEcsFilter(EcsState state)
        {
            _state = state;
        }

        public bool IsMatch(EcsEntity e)
        {
            return !_state.Has<T>(e);
        }
        
        public string GetHash() => $"NOT<{typeof(T).FullName}>";
    }
}