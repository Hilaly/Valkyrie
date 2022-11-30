using System;
using System.Collections.Generic;
using UnityEngine;
using Valkyrie.Utils;

namespace Valkyrie.Cem.Library.CameraFeature
{
    public class CameraFeature : IFeature
    {
        public string Name => "Camera Feature";
        
        public void Import(WorldModelInfo world)
        {
            world.ImportEntity<ICameraPoint>();

            world.ImportSystem<CameraFollowPointSystem>(SimulationOrder.ApplyToView);
        }
    }

    public interface ICameraPoint : I3DPositioned
    {
        
    }
    
    public class CameraFollowPointSystem : BaseTypedSystem<ICameraPoint>
    {
        private ICameraController _cameraController;

        public CameraFollowPointSystem(ICameraController cameraController)
        {
            _cameraController = cameraController;
        }

        protected override void Simulate(float dt, IReadOnlyList<ICameraPoint> entities)
        {
            foreach (var entity in entities)
            {
                var rot = (entity is I3DOriented o) ? o.GetRotation() : Quaternion.identity;
                _cameraController.SetTarget(entity.Position, rot);
                break;
            }
        }
    }
}