using System.Collections.Generic;

namespace Valkyrie.Ecs
{
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
}