using UnityEngine;
using Valkyrie.Playground;

namespace Project.Playground.Features.MonoImplementations
{
    class PhysicMovementComponent : MonoComponent, IPhysicBasedMovementComponent
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Rigidbody Physic => _rigidbody;
    }
}