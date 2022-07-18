using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype
{
    public interface ISceneDataProvider
    {
        Task<List<IEntity>> CreateEntities();
    }

    public class ControlFlow
    {
        private readonly ISceneDataProvider _sceneData;

        public ControlFlow(ISceneDataProvider sceneData)
        {
            _sceneData = sceneData;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            var createdEntities = await _sceneData.CreateEntities();

            foreach (var createdEntity in createdEntities) 
                createdEntity.PropagateEvent(new SpawnedEvent());
        }
    }
}