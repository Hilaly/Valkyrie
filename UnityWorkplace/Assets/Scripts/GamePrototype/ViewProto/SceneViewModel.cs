using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using Unity.VisualScripting;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.MVVM;

namespace GamePrototype.ViewProto
{
    [Binding]
    public class SceneViewModel : MonoBehaviour, ISceneDataProvider
    {
        [Inject] private GameState _gameState;
        [Inject] CameraController _cameraController;
        
        [SerializeField] private SpawnPlayerMarker playerStartPosition;
        [SerializeField] private List<TownMarker> towns;

        public Task PopulateSceneData()
        {
            var gpContext = _gameState.GameplayContext;
            
            var player = gpContext.Create("Player");
            player.AddComponent(new PrefabComponent() { Value = "TestView" });
            player.AddComponent(new MoveCapabilityComponent());
            player.AddComponent(new CameraFollowComponent()
            {
                Value = _cameraController
            });
            player.AddComponent(new ReadKeyboardInputComponent()
            {
                Value = _cameraController.Convert2DInputTo3DDirection
            });

            foreach (var townMarker in towns)
            {
                var town = gpContext.Create(townMarker.name);
                town.AddComponent(new PositionComponent() { Value = townMarker.transform.position });
                town.AddComponent(new PrefabComponent() { Value = "TestView" });
                town.AddComponent(new TownComponent());
                
                (townMarker.GetComponent<EntityHolder>() ?? townMarker.AddComponent<EntityHolder>()).Entity = town;
            }
            
            PropagatePlayerPosition(player);

            return Task.CompletedTask;
        }

        void PropagatePlayerPosition(IEntity player)
        {
            if (!player.HasComponent<PositionComponent>())
                player.AddComponent(new PositionComponent() { Value = playerStartPosition.transform.position });
        }
    }
}