using System.Collections.Generic;
using System.Reflection;

namespace Valkyrie.Ecs
{
    public class GameSimulation
    {
        private readonly List<ISystem> _systems = new();
        private readonly Dictionary<int, List<ISimulationSystem>> _orderedSystems = new();

        public List<ISimulationSystem> GetSimulationSystems(int order)
        {
            lock (_orderedSystems)
            {
                return _orderedSystems.TryGetValue(order, out var result) ? result : default;
            }
        }

        public void Add(ISystem system)
        {
            lock (_systems)
                _systems.Add(system);
            if (system is ISimulationSystem simulationSystem)
                lock (_orderedSystems)
                {
                    var order = GetSystemOrder(simulationSystem);
                    if (!_orderedSystems.TryGetValue(order, out var list))
                        _orderedSystems.Add(order, list = new List<ISimulationSystem>());
                    list.Add(simulationSystem);
                }
        }

        int GetSystemOrder(ISimulationSystem system)
        {
            var systemType = system.GetType();
            var orderAttribute = systemType.GetCustomAttribute<OrderAttribute>()?.Order ?? 0;
            return orderAttribute;
        }
    }
}