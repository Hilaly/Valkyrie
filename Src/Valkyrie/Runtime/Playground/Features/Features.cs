using Valkyrie;
using Feature = Valkyrie.Playground.Feature;

namespace Project.Playground.Features
{
    public class Base3dFeature : Feature
    {
    }

    public class CameraFeature : Feature
    {
        public CameraFeature()
        {
            Register<CameraFollowPointSystem>(SimulationOrder.ApplyToView);
        }
    }

    public class JoystickInputFeature : Feature
    {
        public JoystickInputFeature()
        {
            Register<ReadPlayerInputSystem>(SimulationOrder.ReadPlayerInput);
        }
    }

    public class MovementFeature : Feature
    {
        public MovementFeature()
        {
            Register<ApplyPhysicMovementSystem>(SimulationOrder.ApplyPhysicData + 1);
            
            Register<SimulatePhysicsSystem>(SimulationOrder.SimulatePhysic);
        }
    }
}