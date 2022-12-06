using UnityEngine;

namespace Valkyrie.Playground.Features
{
    class MoveInputComponent : MonoComponent, IMoveInputComponent
    {
        [field:SerializeField] public Vector3 MoveDirection { get; set; }
    }
}