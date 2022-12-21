using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using Unity.VisualScripting;
using UnityEngine;
using Utils;
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
        [SerializeField] private List<NamedEntityPosition> structures;

        [Binding] public IEnumerable<TownViewModel> Towns => _gameState.Towns;
        [Binding] public IEnumerable<BuildingViewModel> Buildings => _gameState.Buildings;

        public Task PopulateSceneData()
        {
            var gpContext = _gameState.GameplayContext;
            
            var player = gpContext.Get("Player");
            PropagatePlayerPosition(player);

            foreach (var positionProvider in structures)
            {
                var e = gpContext.Get(positionProvider.name);
                if(e != null)
                    PropagateEntityPosition(e, positionProvider);
            }
            

            return Task.CompletedTask;
        }

        void PropagateEntityPosition(IEntity entity, Component positionProvider)
        {
            entity.GetOrCreateComponent<PositionComponent>().Value = positionProvider.transform.position;
            entity.GetOrCreateComponent<RotationComponent>().Value = positionProvider.transform.rotation;
        }

        void PropagatePlayerPosition(IEntity player)
        {
            if (!player.HasComponent<PositionComponent>())
                PropagateEntityPosition(player, playerStartPosition);
            
            player.AddComponent(new CameraFollowComponent()
            {
                Value = _cameraController
            });
            player.AddComponent(new ReadKeyboardInputComponent()
            {
                Value = _cameraController.Convert2DInputTo3DDirection
            });
        }
    }
}