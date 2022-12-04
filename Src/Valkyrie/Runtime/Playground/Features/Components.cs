using UnityEngine;
using Valkyrie.Playground;

namespace Project.Playground.Features
{
    public interface ITransformComponent : IComponent
    {
        public Vector3 Position { get; set; }
        public Vector3 Direction { get; set; }
        public Quaternion Rotation { get; set; }
    }

    public interface ICameraPointComponent : IComponent
    {
    }

    public interface IMoveWithJoystickComponent : IComponent
    {
    }

    public interface IMovementAbilityComponent : IComponent
    {
        public float Speed { get; set; }
        Vector3 MoveDirection { get; set; }
    }

    public interface IPhysicBasedMovementComponent : IComponent
    {
        public Rigidbody Physic { get; }
    }

    public interface INameComponent : IComponent
    {
        public string LidName { get; set; }
    }
}