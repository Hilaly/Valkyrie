using UnityEngine;
using Valkyrie.Playground;

namespace Project.Playground.Features.MonoImplementations
{
    class MovementAbilityComponent :MonoComponent, IMovementAbilityComponent
    {
        [field:SerializeField] public float Speed { get; set; }
        [field:SerializeField] public Vector3 MoveDirection { get; set; }
    }
}