using UnityEngine;

namespace Valkyrie.Cem.Library
{
    public class Base3DFeature : IFeature
    {
        public string Name => "3D Base Feature";
        
        public void Import(WorldModelInfo world)
        {
            var posEntity = world.Import<I3DPositioned>();
            var trEntity = world.Import<ITransformable>();
        }
    }

    public interface I3DPositioned : IEntity
    {
        [RequiredProperty] public Vector3 Position { get; set; }
    }
    
    public interface ITransformable : I3DPositioned
    {}
}