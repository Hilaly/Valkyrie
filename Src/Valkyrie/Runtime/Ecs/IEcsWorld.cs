using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsWorld
    {
        IEcsGroups Groups { get; }
        IEcsState State { get; }
        IEcsSystems Systems { get; }

        void Simulate(float dt);
    }
}