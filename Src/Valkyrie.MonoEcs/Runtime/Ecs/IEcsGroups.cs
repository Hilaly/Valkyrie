using System;
using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsGroups
    {
        IGroupBuilder Build();
    }
    
    class EcsGroups : IEcsGroups, IDisposable
    {
        private EcsState _ecsState;
        private readonly Dictionary<string, EcsGroup> _groups = new Dictionary<string, EcsGroup>();
        private EcsEntities _ecsEntities;

        public EcsGroups(EcsState ecsState, EcsEntities ecsEntities)
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