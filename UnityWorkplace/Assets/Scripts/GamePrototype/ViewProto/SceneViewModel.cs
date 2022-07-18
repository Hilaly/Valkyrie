using System.Collections.Generic;
using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Valkyrie.Di;
using Valkyrie.MVVM;

namespace GamePrototype.ViewProto
{
    [Binding]
    public class SceneViewModel : MonoBehaviour, ISceneDataProvider
    {
        [Inject(Name = "CONFIG")] private EntityContext _config;
        [Inject] CameraController _cameraController;
        
        [SerializeField] private SpawnPlayerMarker playerStartPosition;
        
        public Task<List<IEntity>> CreateEntities(EntityContext gpContext)
        {
            var player = gpContext.Create("Player");
            player.AddComponent(new PositionComponent() { Value = playerStartPosition.transform.position });
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

            return Task.FromResult(new List<IEntity>() { player });
        }
    }
}