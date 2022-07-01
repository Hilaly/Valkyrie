using System;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    class EcsGroups : IEcsGroups, IDisposable
    {
        private readonly EcsState _ecsState;
        private readonly Dictionary<string, EcsGroup> _groups = new Dictionary<string, EcsGroup>();
        private readonly EcsState _ecsEntities;

        public EcsGroups(EcsState ecsState, EcsState ecsEntities)
        {
            _ecsState = ecsState;
            _ecsEntities = ecsEntities;
        }

        public IGroupBuilder Build()
        {
            return new GroupBuilder(_groups, _ecsState, _ecsEntities);
        }

        public void Dispose()
        {
            foreach (var @group in _groups)
            {
                group.Value.Dispose();
            }
            _groups.Clear();
        }
    }
}