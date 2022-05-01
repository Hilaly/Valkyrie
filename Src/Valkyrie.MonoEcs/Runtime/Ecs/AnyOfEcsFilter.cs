using System.Collections.Generic;
using System.Linq;

namespace Valkyrie.Ecs
{
    class AnyOfEcsFilter : IEcsFilter
    {
        private readonly List<IEcsFilter> _ecsFilters;

        public AnyOfEcsFilter(IEnumerable<IEcsFilter> ecsFilters)
        {
            _ecsFilters = new List<IEcsFilter>(ecsFilters);
        }

        public AnyOfEcsFilter(params IEcsFilter[] ecsFilters)
        {
            _ecsFilters = new List<IEcsFilter>(ecsFilters);
        }

        public bool IsMatch(EcsEntity e)
        {
            return _ecsFilters.Any(x => x.IsMatch(e));
        }

        public string GetHash() => string.Join("|", _ecsFilters.Select(x => x.GetHash()));
    }
}