using UnityEngine;

namespace Valkyrie.Cem.Library
{
    public class Base3DFeature : IFeature
    {
        public string Name => "3D Base Feature";
        
        public void Import(WorldModelInfo world)
        {
            
        }
    }

    public interface I3DPositioned : IEntity
    {
        public Vector3 Position { get; set; }
    }
}