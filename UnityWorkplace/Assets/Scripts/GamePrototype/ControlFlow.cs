using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype
{
    public interface ISceneDataProvider
    {
        Task<List<IEntity>> CreateEntities(EntityContext gpContext);
    }

    public class ControlFlow
    {
        private readonly EntityContext _ecs;
        private readonly ISceneDataProvider _sceneData;

        public ControlFlow(EntityContext ecs, ISceneDataProvider sceneData)
        {
            _ecs = ecs;
            _sceneData = sceneData;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            var createdEntities = await _sceneData.CreateEntities(_ecs);

            foreach (var createdEntity in createdEntities) 
                createdEntity.PropagateEvent(new SpawnedEvent());
        }
    }
}