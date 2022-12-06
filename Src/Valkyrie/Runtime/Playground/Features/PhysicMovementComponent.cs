using UnityEngine;

namespace Valkyrie.Playground.Features
{
    class PhysicMovementComponent : MonoComponent, IPhysicBasedMovementComponent
    {
        [SerializeField] private Rigidbody rigidbody;

        public Rigidbody Physic => rigidbody;
    }
}