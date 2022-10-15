using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public abstract class EcsFilterSimulationSystem : BaseEcsSystem, IEcsSimulationSystem
    {
        private readonly GroupWrapper _ecsGroup;

        protected EcsFilterSimulationSystem(IEcsWorld ecsWorld)
            : base(ecsWorld)
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            _ecsGroup = Build(Groups.Build()).Wrap();
        }

        protected abstract IGroupBuilder Build(IGroupBuilder builder);

        public void Simulate(float dt) => Simulate(dt, _ecsGroup.Entities);

        protected abstract void Simulate(float dt, List<EcsEntity> ecsEntities);
    }
}