using System;

namespace Valkyrie.Ecs
{
    [Serializable]
    public class SimulationSettings
    {
        public bool IsSimulationPaused;
        public float SimulationSpeed = 1f;
    }
}