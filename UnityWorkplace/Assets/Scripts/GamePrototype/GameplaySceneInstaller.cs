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
            container.Register<EntityContext>()
                .AsInterfacesAndSelf()
                .SingleInstance();
            
            container.RegisterFromComponentOnNewPrefab(cameraController)
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
            
            container.RegisterFromNewComponentOnNewGameObject<Simulator>("Simulator")
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
        }
    }
}