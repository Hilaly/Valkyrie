using System;
using System.Collections.Generic;
using Services;
using UnityEngine;
using Valkyrie.Utils;

namespace Valkyrie.Playground.Features
{
    /// <summary>
    /// Modify move direction with some algo
    /// </summary>
    public interface IMoveDirectionModifierComponent : IComponent
    {
        Vector3 Modify(Vector3 moveDirection);
    }

    /// <summary>
    /// Allow entity to move
    /// </summary>
    public interface IMoveAbilityComponent : IComponent
    {
        public float MoveSpeed { get; set; }
    }

    /// <summary>
    /// Rotate entity to moveDirection
    /// </summary>
    public interface IRotateToMoveDirectionComponent : IComponent
    {
    }

    /// <summary>
    /// Allow entity rotate
    /// </summary>
    public interface IRotationAbilityComponent : IComponent
    {
        public float RotationSpeed { get; set; }
    }

    /// <summary>
    /// Mark this entity to move by physic (collide with environment and other entities)
    /// </summary>
    public interface IPhysicBasedMovementComponent : IComponent
    {
        public Rigidbody Physic { get; }
    }

    /// <summary>
    /// Allow camera to follow this entity
    /// </summary>
    public interface ICameraPointComponent : IComponent
    {
    }

    /// <summary>
    /// Allow entity receive move joystick input
    /// </summary>
    public interface IReadMoveJoystickComponent : IComponent
    {
    }

    /// <summary>
    /// Move Input, read from player
    /// </summary>
    public interface IMoveInputComponent : IComponent
    {
        public Vector3 MoveDirection { get; set; }
    }

    public class ValkyrieFeature : Feature
    {
        public ValkyrieFeature()
        {
            Register<ReadPlayerInputSystem>(SimulationOrder.ReadPlayerInput);

            Register<ApplyPhysicRotateInput>(SimulationOrder.ApplyPhysicData + 1);
            Register<ApplyPhysicMovementSystem>(SimulationOrder.ApplyPhysicData + 1);

            Register<SimulatePhysicsSystem>(SimulationOrder.SimulatePhysic);

            Register<CameraFollowPointSystem>(SimulationOrder.ApplyToView);
        }
    }

    class CameraFollowPointSystem : BaseTypedSystem<ICameraPointComponent, ITransformComponent>
    {
        private readonly ICameraController _cameraController;

        public CameraFollowPointSystem(ICameraController cameraController)
        {
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt,
            IReadOnlyList<Tuple<ICameraPointComponent, ITransformComponent>> entities)
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

    class ReadPlayerInputSystem : BaseTypedSystem<IReadMoveJoystickComponent>
    {
        private readonly IInputService _inputService;
        private readonly ICameraController _cameraController;

        public ReadPlayerInputSystem(IInputService inputService, ICameraController cameraController)
        {
            _inputService = inputService;
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt,
            IReadOnlyList<IReadMoveJoystickComponent> entities)
        {
            var moveInput = _inputService.MoveInput.Value;
            var moveDirection = _cameraController.ConvertToCameraXZ(moveInput);

            foreach (var readMove in entities)
            {
                var input = readMove.Get<IMoveInputComponent>() ?? readMove.Entity.Add<MoveInputComponent>();
                input.MoveDirection = moveDirection;
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

    class ApplyPhysicRotateInput : BaseTypedSystem<IPhysicBasedMovementComponent, IRotateToMoveDirectionComponent,
        IMoveInputComponent>
    {
        protected override void Simulate(float dt,
            IReadOnlyList<Tuple<IPhysicBasedMovementComponent, IRotateToMoveDirectionComponent, IMoveInputComponent>>
                entities)
        {
            for (var index = 0; index < entities.Count; index++)
            {
                var tuple = entities[index];
                //Skip without rigidbody
                var rigidbody = tuple.Item1.Physic;
                if (rigidbody == null)
                    continue;

                //Skip without input
                var moveDirection = tuple.Item3.MoveDirection;
                if (moveDirection.sqrMagnitude < Mathf.Epsilon)
                    continue;

                var resultRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                var rotationAbility = tuple.Item1.Get<IRotationAbilityComponent>();
                if (rotationAbility != null)
                {
                    var sourceRotation =
                        Quaternion.LookRotation(tuple.Item1.Get<IRotationComponent>().Direction, Vector3.up);
                    resultRotation =
                        Quaternion.RotateTowards(sourceRotation, resultRotation, rotationAbility.RotationSpeed * dt);
                }

                rigidbody.MoveRotation(resultRotation);
            }
        }
    }

    class ApplyPhysicMovementSystem : BaseTypedSystem<IPhysicBasedMovementComponent, IMoveAbilityComponent,
        IMoveInputComponent>
    {
        protected override void Simulate(float dt,
            IReadOnlyList<Tuple<IPhysicBasedMovementComponent, IMoveAbilityComponent, IMoveInputComponent>> entities)
        {
            for (var index = 0; index < entities.Count; index++)
            {
                var tuple = entities[index];
                //Skip without rigidbody
                var rigidbody = tuple.Item1.Physic;
                if (rigidbody == null)
                    continue;

                //Skip without input
                var moveDirection = tuple.Item3.MoveDirection;
                if (moveDirection.sqrMagnitude < Mathf.Epsilon)
                    continue;

                //Skip with 0 speed
                var speed = tuple.Item2.MoveSpeed;
                if (speed <= Mathf.Epsilon)
                    continue;

                // Apply modifiers
                foreach (var modifierComponent in tuple.Item1.GetAll<IMoveDirectionModifierComponent>())
                    moveDirection = modifierComponent.Modify(moveDirection);

                var tr = tuple.Item1.Get<ITransformComponent>();
                var deltaPosition = moveDirection * speed * dt;
                var position = tr.Position;
                rigidbody.MovePosition(position + deltaPosition);
            }
        }
    }
}