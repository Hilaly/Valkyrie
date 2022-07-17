using GamePrototype.Mono;
using Hilaly.Utils;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Valkyrie.Di;

namespace GamePrototype
{
    public class GameplaySceneInstaller : MonoBehaviourInstaller
    {
        [SerializeField] private CameraController cameraController;
        
        public override void Register(IContainer container)
        {
            container.Register<ControlFlow>()
                .AsInterfacesAndSelf()
                .OnActivation(async cb =>
                {
                    await cb.Instance.LoadGameplay();
                })
                .SingleInstance()
                .NonLazy();
            
            //Objects from scene
            container.RegisterFromHierarchy<SpawnPlayerMarker>(gameObject.scene)
                .AsInterfacesAndSelf();
            
            //Simulation
            container.RegisterFromNewComponentOnNewGameObject<Simulator>("Simulator")
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
            container.Register<EntityContext>()
                .AsInterfacesAndSelf()
                .SingleInstance();
            
            //Prefabs
            container.RegisterFromComponentOnNewPrefab(cameraController)
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
        }
    }
}