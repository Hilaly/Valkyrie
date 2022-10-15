using System;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using EntitiesSerializer = NaiveEntity.GamePrototype.EntProto.EntitiesSerializer;

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
        private readonly EntitiesSerializer _es;

        public ControlFlow(ISceneDataProvider sceneData, GameState gameState, EntitiesSerializer es)
        {
            _sceneData = sceneData;
            _gameState = gameState;
            _es = es;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            try
            {
                _es.Deserialize(_gameState.GameplayContext, Resources.Load<TextAsset>("Config").text);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                throw;
            }

            await SpawnAllEntities();

            await _sceneData.PopulateSceneData();

            foreach (var createdEntity in _gameState.GameplayContext.Get())
                createdEntity.PropagateEvent(new SpawnedEvent());
        }

        private Task SpawnAllEntities()
        {
            var gpContext = _gameState.GameplayContext;

            var player = gpContext.Create("Player");
            player.AddComponent(new PrefabComponent() { Value = "TestView" });
            player.AddComponent(new MoveCapabilityComponent());
            player.AddComponent(new PlayerEnterTriggerComponent());
            player.AddComponent(new SpawnPrefabComponent());

            return Task.CompletedTask;
        }
    }
}