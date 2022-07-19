using System;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Valkyrie.Entities;
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

        public ControlFlow(ISceneDataProvider sceneData, GameState gameState)
        {
            _sceneData = sceneData;
            _gameState = gameState;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            var es = new EntitiesSerializer();
            try
            {
                es.Deserialize(_gameState.GameplayContext, Resources.Load<TextAsset>("Config").text);
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