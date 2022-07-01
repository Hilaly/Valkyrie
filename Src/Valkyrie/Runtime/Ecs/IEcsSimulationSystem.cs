namespace Valkyrie.Ecs
{
    public interface IEcsSimulationSystem : IEcsSystem
    {
        void Simulate(float dt);
    }
}