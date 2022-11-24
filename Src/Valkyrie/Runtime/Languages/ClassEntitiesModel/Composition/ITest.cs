using System.Collections.Generic;
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
        public ITest Data { get; set; }
        public float DT { get; set; }
    }
    
    public interface ITestInitSystem : ISharedSystem, IEcsInitSystem
    {}
    
    public interface ITestSystem : IArchetypeSimSystem<ITest>, IArchetypeEntitySimSystem<ITest> { }
    
    public interface ITestSimSystem : ISimSystem
    {}

    public class TestSystem : ITestSystem, IEcsSimulationSystem
    {
        public void Simulate(IReadOnlyList<ITest> e, float dt)
        {
            throw new System.NotImplementedException();
        }

        public void Simulate(ITest e, float dt)
        {
            throw new System.NotImplementedException();
        }

        public void Simulate(float dt)
        {
            throw new System.NotImplementedException();
        }
    }

    public interface ITestEventSystem : IEventSystem<ITestEvent>
    {
        
    }
}