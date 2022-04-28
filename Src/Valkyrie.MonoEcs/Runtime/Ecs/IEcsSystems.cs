using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsSystems
    {
        void Add(IEcsSystem ecsSystem);
    }

    class EcsSystems : IEcsSystems
    {
        private readonly List<IEcsSystem> _systems = new List<IEcsSystem>();
        private readonly List<IEcsSimulationSystem> _simulationSystems = new List<IEcsSimulationSystem>();

        public void Add(IEcsSystem ecsSystem)
        {
            _systems.Add(ecsSystem);
            if (ecsSystem is IEcsSimulationSystem simSystem)
                _simulationSystems.Add(simSystem);
        }

        public void Simulate(float deltaTime)
        {
            for (var i = 0; i < _simulationSystems.Count; ++i)
                _simulationSystems[i].Simulate(deltaTime);
        }
    }

    public interface IEcsSystem
    {
    }

    public interface IEcsSimulationSystem
    {
        void Simulate(float dt);
    }

    public abstract class BaseEcsSystem : IEcsSystem
    {
        public IEcsState State { get; }
        public IEcsGroups Groups { get; }
        public IEcsEntities Entities { get; }

        protected BaseEcsSystem(IEcsWorld ecsWorld)
        {
            State = ecsWorld.State;
            Groups = ecsWorld.Groups;
            Entities = ecsWorld.Entities;
        }
    }

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