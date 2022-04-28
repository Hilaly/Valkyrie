using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsWorld
    {
        IEcsGroups Groups { get; }
        IEcsEntities Entities { get; }
        IEcsState State { get; }
        IEcsSystems Systems { get; }

        void Simulate(float dt);
    }
    
    public class EcsWorld : IEcsWorld
    {
        private readonly EcsEntities _ecsEntities;
        private readonly EcsState _ecsState;
        private readonly EcsGroups _ecsGroups;
        private readonly EcsSystems _ecsSystems;

        public IEcsEntities Entities => _ecsEntities;
        public IEcsGroups Groups => _ecsGroups;
        public IEcsState State => _ecsState;
        public IEcsSystems Systems => _ecsSystems;

        public EcsWorld()
        {
            _ecsEntities = new EcsEntities();
            _ecsState = new EcsState();
            _ecsGroups = new EcsGroups(_ecsState, _ecsEntities);
            _ecsSystems = new EcsSystems();
        }
        
        public void Simulate(float dt)
        {
            _ecsSystems.Simulate(dt);
        }
    }
}