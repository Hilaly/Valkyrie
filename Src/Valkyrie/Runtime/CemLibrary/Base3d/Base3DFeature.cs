using System.Collections.Generic;
using UnityEngine;

namespace Valkyrie.Cem.Library
{
    public class Base3DFeature : IFeature
    {
        public string Name => "3D Base Feature";

        public void Import(WorldModelInfo world)
        {
            var posEntity = world.ImportEntity<I3DPositioned>();
            var rotEntity = world.ImportEntity<I3DOriented>()
                .AddInfo(typeof(Quaternion).FullName, "Rotation",
                    $"{typeof(Quaternion).FullName}.LookRotation(Direction, Vector3.up)");
            var trEntity = world.ImportEntity<ITransformable>();

            world.ImportSystem<TestSystem>();
        }
    }

    public interface I3DPositioned : IEntity
    {
        [RequiredProperty] public Vector3 Position { get; set; }
    }

    public interface I3DOriented : IEntity
    {
        [RequiredProperty] public Vector3 Direction { get; set; }
    }

    public interface ITransformable : I3DPositioned, I3DOriented
    {
    }
    
    public class TestSystem : BaseTypedSystem<ITransformable>
    {
        protected override void Simulate(float dt, IReadOnlyList<ITransformable> entities)
        {
            Debug.Log($"Sim {entities.Count} entities");
        }
    }
}