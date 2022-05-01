using System.Collections.Generic;

namespace Valkyrie.Ecs
{
    public interface IEcsGroup : IEnumerable<EcsEntity>
    {
        int Count { get; }

        List<EcsEntity> GetEntities(List<EcsEntity> buffer);
    }
}