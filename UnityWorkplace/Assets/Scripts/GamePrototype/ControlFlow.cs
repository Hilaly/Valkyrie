using System.Threading.Tasks;
using GamePrototype.GameLogic;
using GamePrototype.Mono;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;

namespace GamePrototype
{
    public class ControlFlow
    {
        private readonly SpawnPlayerMarker _spawnPlayerMarker;
        private readonly EntityContext _ecs;

        public ControlFlow(SpawnPlayerMarker spawnPlayerMarker, EntityContext ecs)
        {
            _spawnPlayerMarker = spawnPlayerMarker;
            _ecs = ecs;
        }

        public async Task LoadGameplay()
        {
            Debug.LogWarning($"Loading GP");

            var player = _ecs.Create("Player");
            player.AddComponent(new PositionComponent() { Value = _spawnPlayerMarker.transform.position });
        }
    }
}