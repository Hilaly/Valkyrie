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
        private readonly IWorldFilter<ITestEvent> _filter;

        public TestSystem(IWorldFilter<ITestEvent> filter)
        {
            _filter = filter;
        }

        public void Simulate(IReadOnlyList<ITest> e, float dt)
        {
            foreach (var test in e) 
                Simulate(test, dt);
        }

        public void Simulate(ITest e, float dt)
        {
            Debug.Log($"Sim {e.Marker}");
        }

        public void Simulate(float dt)
        {
            foreach (var testEvent in _filter.GetAll())
            {
                //testEvent.Data.Destroy();
            }
        }
    }

    public interface ITestEventSystem : IEventSystem<ITestEvent>
    {
        
    }
}