using UnityEngine;

namespace Valkyrie.Playground.Features
{
    public class TimerFinishedEvent : IEventComponent
    {
        public IEntity Entity { get; }
        
        public ITimerComponent TimerComponent { get; set; }
        public IEntity SourceEntity { get; set; }
    }

    public class TimerComponent : MonoComponent, ITimerComponent
    {
        [field:SerializeField] public float FullTime { get; private set; }
        [field:SerializeField] public float TimeLeft { get; private set; }

        public void StartTimer(float time) => FullTime = TimeLeft = time;
        public void AdvanceTimer(float dt) => TimeLeft = Mathf.Clamp(TimeLeft - dt, 0f, FullTime);
    }
}