using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Valkyrie.Cem.Library.Tracking
{
    public class TrackingFeature : IFeature
    {
        public string Name => "Tracking feature";

        public void Import(WorldModelInfo world)
        {
            world.ImportEntity<IEntitiesTracker>();
            world.ImportEntity<ISingleEntityTracker>();

            world.ImportSystem<CollectTargetsSystem>(SimulationOrder.ReadPhysicData + 1);
            world.ImportSystem<SelectTargetSystem>(SimulationOrder.ReadPhysicData + 1);
        }
    }

    public interface ITrackData
    {
        public float TrackRange { get; }
        public Func<I3DPositioned, bool> Filter { get; }
        public List<I3DPositioned> TrackedTargets { get; }
    }

    public interface ISingleTargetTrackData : ITrackData
    {
        
    }

    /// <summary>
    /// This entity tracks targets in range
    /// </summary>
    public interface IEntitiesTracker : I3DPositioned
    {
        public float TrackRange { get; }

        public List<I3DPositioned> Targets { get; set; }
        public Func<I3DPositioned, bool> Filter { get; }
    }

    public interface ISingleEntityTracker : IEntitiesTracker
    {
        public bool CanChangeTarget { get; }
        public Func<ISingleEntityTracker, I3DPositioned, float> TargetWeight { get; }
        public I3DPositioned SelectedTarget { get; set; }
    }

    public class CollectTargetsSystem : BaseTypedSystem<IEntitiesTracker>
    {
        private readonly List<KeyValuePair<I3DPositioned, float>> _distancesCache = new(10);

        private readonly IWorldFilter<I3DPositioned> _worldFilter;

        public CollectTargetsSystem(IWorldFilter<I3DPositioned> worldFilter)
        {
            _worldFilter = worldFilter;
        }

        protected override void Simulate(float dt, IReadOnlyList<IEntitiesTracker> entities)
        {
            var targets = _worldFilter.GetAll();

            foreach (var tracker in entities)
            {
                if (tracker.Targets == null)
                    tracker.Targets = new List<I3DPositioned>();
                else
                    tracker.Targets.Clear();
                _distancesCache.Clear();

                var sqrRange = tracker.TrackRange * tracker.TrackRange;

                for (var index = 0; index < targets.Count; ++index)
                {
                    var target = targets[index];
                    
                    //Filter null and self
                    if(target == null || target == tracker)
                        continue;
                    
                    //Use filter for other
                    if(tracker.Filter != null && !tracker.Filter(target))
                        continue;

                    //Filter that are out of range
                    var sqrDistance = (target.Position - tracker.Position).sqrMagnitude;
                    if(sqrDistance > sqrRange)
                        continue;

                    var added = false;
                    for (var i = 0; i < _distancesCache.Count; ++i)
                    {
                        if (_distancesCache[i].Value < sqrDistance)
                            continue;
                        
                        _distancesCache.Insert(i, new KeyValuePair<I3DPositioned, float>(target, sqrDistance));
                        tracker.Targets.Insert(i, target);
                        added = true;
                        break;
                    }

                    if (added)
                        continue;
                    
                    _distancesCache.Add(new KeyValuePair<I3DPositioned, float>(target, sqrDistance));
                    tracker.Targets.Add(target);
                }
                
                /*Debug ogs
                Debug.Assert(_distancesCache.Count == tracker.Targets.Count);
                for(var i = 0; i < _distancesCache.Count; ++i)
                    Debug.Assert(_distancesCache[i].Key == tracker.Targets[i]);

                Debug.LogWarning($"[TRACK]: targets={string.Join(",", tracker.Targets)}");
                */
            }
            
            _distancesCache.Clear();
        }
    }
    
    public class SelectTargetSystem : BaseTypedSystem<ISingleEntityTracker>
    {
        protected override void Simulate(float dt, IReadOnlyList<ISingleEntityTracker> entities)
        {
            foreach (var tracker in entities)
            {
                //Reset target if lost
                if(tracker.SelectedTarget != null)
                    if (!tracker.Targets.Contains(tracker.SelectedTarget))
                        tracker.SelectedTarget = null;

                if (tracker.SelectedTarget != null && !tracker.CanChangeTarget) 
                    continue;
                
                if (tracker.TargetWeight != null)
                    tracker.SelectedTarget = tracker.Targets
                        .OrderByDescending(x => tracker.TargetWeight(tracker, x)).FirstOrDefault();
                else if (tracker.Targets.Count > 0)
                    tracker.SelectedTarget = tracker.Targets[0];
                else
                    tracker.SelectedTarget = null;
            }

            foreach (var tracker in entities)
            {
                Debug.LogWarning($"[TRACK]: Selected={tracker.SelectedTarget}");
            }
        }
    }
}