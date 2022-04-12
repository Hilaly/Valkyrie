namespace Valkyrie.Ecs
{
    public interface ISimulationComponent : IComponent
    {
        void DoUpdate(float dt);
    }
}