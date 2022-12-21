namespace Valkyrie.Ecs
{
    public interface ISimulationSystem : ISystem
    {
        void DoUpdate(float dt);
    }
}