using Project.Playground.Features;
using UnityEngine;

namespace Valkyrie.Playground.Features.MonoImplementations
{
    class MovementAbilityComponent :MonoComponent, IMovementAbilityComponent
    {
        [field:SerializeField] public float Speed { get; set; }
        [field:SerializeField] public Vector3 MoveDirection { get; set; }
    }
}