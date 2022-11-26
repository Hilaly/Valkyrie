using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Cem.Library.Moves
{
    public class Moving3dFeature : IFeature
    {
        public string Name => "3D moving";

        public void Import(WorldModelInfo world)
        {
            world.ImportEntity<IPhysicMovement>();
            
            world.ImportSystem<ApplyPhysicMovementSystem>(SimulationOrder.ApplyPhysicData + 1);
            world.ImportSystem<ReadPositionFromPhysicSystem>(SimulationOrder.ReadPhysicData - 1);
        }
    }

    public interface IPhysicMovement : IEntity
    {
        public Vector3 MoveDirection { get; set; }
        public Rigidbody Physic { get; set; }
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
                    //TODO: read speed somewhere
                    var speed = 1f;
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