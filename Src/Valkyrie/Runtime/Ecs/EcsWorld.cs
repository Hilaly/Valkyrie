namespace Valkyrie.Ecs
{
    public class EcsWorld : IEcsWorld
    {
        private readonly EcsState _ecsState;
        private readonly EcsGroups _ecsGroups;
        private readonly EcsSystems _ecsSystems;

        public IEcsGroups Groups => _ecsGroups;
        public IEcsState State => _ecsState;
        public IEcsSystems Systems => _ecsSystems;

        public EcsWorld()
        {
            _ecsState = new EcsState();
            _ecsSystems = new EcsSystems();
            _ecsGroups = new EcsGroups(_ecsState, _ecsState);
        }
        
        public void Simulate(float dt)
        {
            _ecsSystems.Simulate(dt);
        }
    }
}