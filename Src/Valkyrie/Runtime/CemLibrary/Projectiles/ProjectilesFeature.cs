using System;
using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Cem.Library.Projectiles
{
    public class ProjectilesFeature : IFeature
    {
        public string Name => "Projectiles";

        public void Import(WorldModelInfo world)
        {
            world.ImportSystem<FollowTargetProjectileSystem>();
            world.ImportSystem<MoveProjectilesSystem>();
        }
    }

    public interface IProjectile : ITransformable
    {
        [RequiredProperty] public float Speed { get; set; }
    }

    public interface ITargetedProjectile : IProjectile
    {
        [RequiredProperty] public I3DPositioned Target { get; set; }
        public Vector3 TargetPosition { get; set; }
    }

    public class FollowTargetProjectileSystem : BaseTypedSystem<ITargetedProjectile>
    {
        protected override void Simulate(float dt, IReadOnlyList<ITargetedProjectile> entities)
        {
            foreach (var projectile in entities)
            {
                if (projectile.Target == null)
                    continue;

                projectile.TargetPosition = projectile.Target.Position;
                projectile.Direction = projectile.TargetPosition - projectile.Position;
            }
        }
    }

    public class MoveProjectilesSystem : BaseTypedSystem<IProjectile>
    {
        protected override void Simulate(float dt, IReadOnlyList<IProjectile> entities)
        {
            foreach (var projectile in entities)
            {
                var moveMagnitude = dt * projectile.Speed;
                if (projectile is ITargetedProjectile targetedProjectile)
                    projectile.Position = Vector3.MoveTowards(projectile.Position, targetedProjectile.TargetPosition,
                        moveMagnitude);
                else
                    projectile.Position += projectile.Direction.normalized * moveMagnitude;
            }
        }
    }
}