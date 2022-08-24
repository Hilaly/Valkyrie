using System;
using UnityEngine;

namespace Valkyrie.Ecs
{
    [Serializable]
    public class SimulationSettings
    {
        public bool IsSimulationPaused;
        public float SimulationSpeed = 1f;
        [Range(0,1000)]
        public int SimulationFreuency = 60;

        public float SimTickTime => 1f / SimulationFreuency;
    }
}