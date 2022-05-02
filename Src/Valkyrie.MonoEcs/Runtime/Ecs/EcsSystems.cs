using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    class EcsSystems : IEcsSystems
    {
        private readonly List<IEcsSystem> _systems = new List<IEcsSystem>();
        private readonly List<IEcsSimulationSystem> _simulationSystems = new List<IEcsSimulationSystem>();
        private readonly List<IEcsInitSystem> _initSystems = new List<IEcsInitSystem>();
        private readonly List<IEcsCleanupSystem> _cleanupSystems = new List<IEcsCleanupSystem>();

        public void Add(IEcsSystem ecsSystem)
        {
            _systems.Add(ecsSystem);
            if (ecsSystem is IEcsInitSystem initSystem)
                _initSystems.Add(initSystem);
            if (ecsSystem is IEcsSimulationSystem simSystem)
                _simulationSystems.Add(simSystem);
            if (ecsSystem is IEcsCleanupSystem cleanupSystem)
                _cleanupSystems.Add(cleanupSystem);
        }

        public void Simulate(float deltaTime)
        {
            while (_initSystems.Count > 0)
            {
                var system = _initSystems[0];
                _initSystems.RemoveAt(0);
                system.Init();
            }

            for (var i = 0; i < _simulationSystems.Count; ++i)
                _simulationSystems[i].Simulate(deltaTime);

            for (int i = 0; i < _cleanupSystems.Count; i++)
                _cleanupSystems[i].Clean();
        }
    }
}