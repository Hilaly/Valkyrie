using System;
using System.Collections.Generic;
using Services;
using UnityEngine;
using Valkyrie.Playground;
using Valkyrie.Utils;

namespace Project.Playground.Features
{
    class CameraFollowPointSystem : BaseTypedSystem<ICameraPointComponent, ITransformComponent>
    {
        private readonly ICameraController _cameraController;

        public CameraFollowPointSystem(ICameraController cameraController)
        {
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt, IReadOnlyList<Tuple<ICameraPointComponent, ITransformComponent>> entities)
        {
            for (var index = 0; index < entities.Count;)
            {
                var entity = entities[index].Item2;
                var rot = entity.Rotation;
                _cameraController.SetTarget(entity.Position, rot);
                break;
            }
        }
    }

    class ReadPlayerInputSystem : BaseTypedSystem<IMoveWithJoystickComponent, IMovementAbilityComponent>
    {
        private readonly IInputService _inputService;
        private readonly ICameraController _cameraController;

        public ReadPlayerInputSystem(IInputService inputService, ICameraController cameraController)
        {
            _inputService = inputService;
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt, IReadOnlyList<Tuple<IMoveWithJoystickComponent, IMovementAbilityComponent>> entities)
        {
            var moveInput = _inputService.MoveInput.Value;
            var moveDirection = _cameraController.ConvertToCameraXZ(moveInput);

            foreach (var (_, movable) in entities)
                movable.MoveDirection = moveDirection;
        }
    }

    class ApplyPhysicMovementSystem : BaseTypedSystem<IPhysicBasedMovementComponent, ITransformComponent, IMovementAbilityComponent>
    {
        protected override void Simulate(float dt, IReadOnlyList<Tuple<IPhysicBasedMovementComponent, ITransformComponent, IMovementAbilityComponent>> entities)
        {
            for (var index = 0; index < entities.Count; index++)
            {
                var entity = entities[index];
                if (entity.Item1.Physic == null)
                    continue;
                if (entity.Item3.MoveDirection.sqrMagnitude < Mathf.Epsilon)
                    continue;

                var tr = entity.Item2;
                
                var fromRotation = Quaternion.LookRotation(tr.Direction, Vector3.up);
                var toRotation = Quaternion.LookRotation(entity.Item3.MoveDirection, Vector3.up);
                //TODO: find rotation speed
                var maxDegrees = 360f * dt;
                var resultRotation = Quaternion.RotateTowards(fromRotation, toRotation, maxDegrees);
                entity.Item1.Physic.MoveRotation(resultRotation);

                var speed = entity.Item3.Speed;
                if (speed > Mathf.Epsilon)
                {
                    var deltaPosition = entity.Item3.MoveDirection.normalized * speed * dt;
                    entity.Item1.Physic.MovePosition(tr.Position + deltaPosition);
                }
            }
        }
    }

    class SimulatePhysicsSystem : ISystem
    {
        public SimulatePhysicsSystem()
        {
            Physics.autoSimulation = false;
        }

        public void Simulate(float dt)
        {
            Physics.Simulate(dt);
        }
    }
}