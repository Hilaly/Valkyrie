using System.Threading.Tasks;
using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype
{
    public interface ISceneDataProvider
    {
        Task PopulateSceneData();
    }

    public class ControlFlow
    {
        private readonly ISceneDataProvider _sceneData;
        private readonly GameState _gameState;

        public ControlFlow(ISceneDataProvider sceneData, GameState gameState)
        {
            _sceneData = sceneData;
            _gameState = gameState;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            await _sceneData.PopulateSceneData();

            foreach (var createdEntity in _gameState.GameplayContext.Get()) 
                createdEntity.PropagateEvent(new SpawnedEvent());
        }
    }
}