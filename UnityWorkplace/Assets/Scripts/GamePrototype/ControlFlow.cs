using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype
{
    public class ControlFlow
    {
        private readonly SpawnPlayerMarker _spawnPlayerMarker;
        private readonly EntityContext _ecs;
        private readonly CameraController _cameraController;

        public ControlFlow(SpawnPlayerMarker spawnPlayerMarker, EntityContext ecs, CameraController cameraController)
        {
            _spawnPlayerMarker = spawnPlayerMarker;
            _ecs = ecs;
            _cameraController = cameraController;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            var player = _ecs.Create("Player");
            player.AddComponent(new PositionComponent() { Value = _spawnPlayerMarker.transform.position });
            player.AddComponent(new PrefabComponent() { Value = "TestView" });
            player.AddComponent(new MoveCapabilityComponent());
            player.AddComponent(new ReadKeyboardInputComponent()
            {
                InputConverter = _cameraController.Convert2DInputTo3DDirection
            });
            player.PropagateEvent(SpawnedEvent.Instance);
        }
    }
}