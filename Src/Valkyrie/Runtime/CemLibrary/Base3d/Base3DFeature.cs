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
            var trEntity = world.ImportEntity<I3DTransform>();
        }
    }

    /// <summary>
    /// Entity with 3d position
    /// </summary>
    public interface I3DPositioned : IEntity
    {
        [RequiredProperty] public Vector3 Position { get; set; }
    }

    /// <summary>
    /// Entity with 3d Orientation
    /// </summary>
    public interface I3DOriented : IEntity
    {
        [RequiredProperty] public Vector3 Direction { get; set; }
    }

    /// <summary>
    /// Entity with position and orientation
    /// </summary>
    public interface I3DTransform : I3DPositioned, I3DOriented
    {
    }

    public static class Base3DExt
    {
        public static Quaternion GetRotation(this I3DOriented oriented) =>
            Quaternion.LookRotation(oriented.Direction, Vector3.up);

        public static Quaternion SetRotation(this I3DOriented oriented, Quaternion rotation)
        {
            oriented.Direction = rotation * Vector3.forward;
            return oriented.GetRotation();
        }
    }
}