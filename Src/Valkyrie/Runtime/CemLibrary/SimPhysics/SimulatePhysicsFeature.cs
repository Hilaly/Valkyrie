using UnityEngine;

namespace Valkyrie.Cem.Library
{
    public class SimulatePhysicsFeature : IFeature
    {
        public string Name => "Simulate physics feature";
        
        public void Import(WorldModelInfo world)
        {
            world.ImportSystem<SimulatePhysicsSystem>(SimulationOrder.SimulatePhysic);
        }
    }
    
    public class SimulatePhysicsSystem : ISimSystem
    {
        public SimulatePhysicsSystem()
        {
            Physics.autoSimulation = false;
        }

        public void Simulate(float dt)
        {
            Physics.Simulate(dt);
        }
    }
}