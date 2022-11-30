using System.Collections.Generic;
using Services;
using UnityEngine;
using Valkyrie.Utils;

namespace Valkyrie.Cem.Library.ReadPlayerInput
{
    public class InputFeature : IFeature
    {
        public string Name => "Joystick Input Feature";

        public void Import(WorldModelInfo world)
        {
            world.ImportEntity<IInputConsumer>();
            world.ImportEntity<IMoveWithJoystick>();

            world.ImportSystem<ReadPlayerInputSystem>(SimulationOrder.ReadPlayerInput);
        }
    }

    /// <summary>
    /// Marker interface for entities, which receive player input
    /// </summary>
    public interface IInputConsumer : IEntity
    {
    }

    /// <summary>
    /// Use for direct settings joystick input to entity
    /// </summary>
    public interface IMoveWithJoystick : IInputConsumer
    {
        public Vector3 MoveDirection { get; set; }
    }

    public class ReadPlayerInputSystem : BaseTypedSystem<IInputConsumer>
    {
        private readonly IInputService _inputService;
        private readonly ICameraController _cameraController;

        public ReadPlayerInputSystem(IInputService inputService, ICameraController cameraController)
        {
            _inputService = inputService;
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt, IReadOnlyList<IInputConsumer> entities)
        {
            var moveInput = _inputService.MoveInput.Value;
            var moveDirection = _cameraController.ConvertToCameraXZ(moveInput);

            for (var index = 0; index < entities.Count; index++)
            {
                var consumer = entities[index];
                if (consumer is IMoveWithJoystick moveWithJoystick)
                    moveWithJoystick.MoveDirection = moveDirection;
            }
        }
    }
}