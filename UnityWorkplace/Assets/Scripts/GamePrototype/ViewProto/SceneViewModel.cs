using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using NaiveEntity.GamePrototype.EntProto.ViewProto;
using Unity.VisualScripting;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.MVVM;

namespace GamePrototype.ViewProto
{
    [Binding]
    public class SceneViewModel : MonoBehaviour, ISceneDataProvider
    {
        [Inject] private IConfig _config;
        [Inject] CameraController _cameraController;
        
        [SerializeField] private SpawnPlayerMarker playerStartPosition;
        [SerializeField] private List<TownMarker> towns;

        public Task<List<IEntity>> CreateEntities(EntityContext gpContext)
        {
            var result = new List<IEntity>();
            
            var player = gpContext.Create("Player");
            result.Add(player);
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
                result.Add(town);

                town.AddComponent(new PositionComponent() { Value = townMarker.transform.position });
                town.AddComponent(new PrefabComponent() { Value = "TestView" });
                
                (townMarker.GetComponent<EntityHolder>() ?? townMarker.AddComponent<EntityHolder>()).Entity = town;
            }
            
            PropagatePlayerPosition(player);
            
            

            return Task.FromResult(result);
        }

        void PropagatePlayerPosition(IEntity player)
        {
            if (!player.HasComponent<PositionComponent>())
                player.AddComponent(new PositionComponent() { Value = playerStartPosition.transform.position });
        }
    }
}