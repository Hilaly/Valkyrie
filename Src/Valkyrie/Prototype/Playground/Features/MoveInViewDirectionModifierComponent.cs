using UnityEngine;
using Utils;

namespace Valkyrie.Playground.Features
{
    class MoveInViewDirectionModifierComponent : MonoComponent, IMoveDirectionModifierComponent
    {
        [SerializeField] private float maxAllowedAngle = 1f;

        public Vector3 Modify(Vector3 moveDirection)
        {
            var currentDirection = transform.forward.X0Z();
            var targetDirection = moveDirection;
            
            return Quaternion.RotateTowards(
                Quaternion.LookRotation(currentDirection, Vector3.up),
                Quaternion.LookRotation(targetDirection, Vector3.up),
                maxAllowedAngle) * Vector3.forward * moveDirection.magnitude;
        }
    }
}