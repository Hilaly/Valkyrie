using Project.Playground.Features;
using UnityEngine;

namespace Valkyrie.Playground.Features.MonoImplementations
{
    class PhysicMovementComponent : MonoComponent, IPhysicBasedMovementComponent
    {
        [SerializeField] private Rigidbody _rigidbody;

        public Rigidbody Physic => _rigidbody;
    }
}