using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Cem.Library.Moves
{
    public class Moving3dFeature : IFeature
    {
        public string Name => "3D moving";

        public void Import(WorldModelInfo world)
        {
            world.ImportEntity<IMovable>();
            world.ImportEntity<IPhysicMovement>();
            
            world.ImportSystem<ApplyPhysicMovementSystem>(SimulationOrder.ApplyPhysicData + 1);
            world.ImportSystem<ReadPositionFromPhysicSystem>(SimulationOrder.ReadPhysicData - 1);
        }
    }

    /// <summary>
    /// Base movable entity, speed parameter exist
    /// </summary>
    public interface IMovable : IEntity
    {
        public float Speed { get; }
    }

    /// <summary>
    /// Entity, which move with physic
    /// </summary>
    public interface IPhysicMovement : IMovable
    {
        public Vector3 MoveDirection { get; }
        public Rigidbody Physic { get; }
    }

    public class ApplyPhysicMovementSystem : BaseTypedSystem<IPhysicMovement>
    {
        protected override void Simulate(float dt, IReadOnlyList<IPhysicMovement> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Physic == null)
                    continue;
                if (entity.MoveDirection.sqrMagnitude < Mathf.Epsilon)
                    continue;

                if (entity is I3DOriented oriented)
                {
                    var fromRotation = Quaternion.LookRotation(oriented.Direction, Vector3.up);
                    var toRotation = Quaternion.LookRotation(entity.MoveDirection, Vector3.up);
                    //TODO: find rotation speed
                    var maxDegrees = 360f * dt;
                    var resultRotation = Quaternion.RotateTowards(fromRotation, toRotation, maxDegrees);
                    entity.Physic.MoveRotation(resultRotation);
                }

                if (entity is I3DPositioned positioned)
                {
                    var speed = entity.Speed;
                    if (speed > Mathf.Epsilon)
                    {
                        var deltaPosition = entity.MoveDirection.normalized * speed * dt;
                        entity.Physic.MovePosition(positioned.Position + deltaPosition);
                    }
                }
            }
        }
    }

    public class ReadPositionFromPhysicSystem : BaseTypedSystem<IPhysicMovement>
    {
        protected override void Simulate(float dt, IReadOnlyList<IPhysicMovement> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.Physic == null)
                    continue;

                if (entity is I3DPositioned positioned)
                    positioned.Position = entity.Physic.position;
                if (entity is I3DOriented oriented)
                    oriented.SetRotation(entity.Physic.rotation);
            }
        }
    }
}