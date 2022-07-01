using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public abstract class EcsFilterSimulationSystem : BaseEcsSystem, IEcsSimulationSystem
    {
        private readonly IEcsGroup _ecsGroup;
        private readonly List<EcsEntity> _buffer = new List<EcsEntity>();

        protected EcsFilterSimulationSystem(IEcsWorld ecsWorld)
            : base(ecsWorld)
        {
            _ecsGroup = Build(Groups.Build()).Build();
        }

        protected abstract IGroupBuilder Build(IGroupBuilder builder);

        public void Simulate(float dt)
        {
            var list = _ecsGroup.GetEntities(_buffer);
            Simulate(dt, list);
        }

        protected abstract void Simulate(float dt, List<EcsEntity> ecsEntities);
    }
}