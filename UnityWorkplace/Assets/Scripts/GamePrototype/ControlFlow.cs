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

            foreach (var s in new string[] { "A", "B"})
            {
                var town = gpContext.Create(s);
                town.AddComponent(new TownComponent());
            }

            foreach (var s in new string[]{ "B.House1"})
            {
                var b = gpContext.Create(s);
                b.AddComponent(new BuildingComponent());
                b.AddComponent(new PrefabComponent() { Value = "House" });
            }

            return Task.CompletedTask;
        }
    }
}