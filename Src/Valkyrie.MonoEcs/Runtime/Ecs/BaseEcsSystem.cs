namespace Valkyrie.Ecs
{
    public abstract class BaseEcsSystem : IEcsSystem
    {
        public IEcsState State { get; }
        public IEcsGroups Groups { get; }

        protected BaseEcsSystem(IEcsWorld ecsWorld)
        {
            State = ecsWorld.State;
            Groups = ecsWorld.Groups;
        }
    }
}