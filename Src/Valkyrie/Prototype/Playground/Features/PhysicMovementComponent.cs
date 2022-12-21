using System;
using UnityEngine;

namespace Valkyrie.Playground.Features
{
    class PhysicMovementComponent : MonoComponent, IPhysicBasedMovementComponent
    {
        [SerializeField] private Rigidbody rigidbody;

        private void Awake()
        {
            if(rigidbody == null)
                rigidbody = GetComponent<Rigidbody>();
        }

        public Rigidbody Physic => rigidbody;
    }
}