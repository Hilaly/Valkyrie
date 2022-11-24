using UnityEngine;
using Valkyrie.Ecs;

namespace Valkyrie.Composition
{
    [RequiredProperty(nameof(Position))]
    [ExcludeProperty(nameof(Timer))]
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

    public interface ITestEvent : IEventEntity
    {
        public object Data { get; set; }
    }
    
    public interface ITestInitSystem : ISharedSystem, IEcsInitSystem
    {}
    
    public interface ITestSystem : IArchetypeSimSystem<ITest>, IArchetypeEntitySimSystem<ITest>
    {
        
    }

    public interface ITestEventSystem : IEventSystem<ITestEvent>
    {
        
    }
}