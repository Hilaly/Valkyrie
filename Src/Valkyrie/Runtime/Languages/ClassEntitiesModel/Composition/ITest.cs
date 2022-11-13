using UnityEngine;

namespace Valkyrie.Composition
{
    public interface ITest : IEntity
    {
        public bool GetMarker { get; }
        public bool SetMarker { set; }
        public bool Marker { get; set; }
        
        public Vector3 Position { get; set; }
        public Vector3 GetPosition { get; }
        public Vector3 SetPosition { set; }
        
        public ITimer Timer { get; }
    }

    public interface ITestSystem : ISimSystem
    {
        
    }
}