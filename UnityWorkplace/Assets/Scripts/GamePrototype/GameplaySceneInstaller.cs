using Hilaly.Utils;
using UnityEngine;
using Valkyrie.Di;

namespace GamePrototype
{
    public class GameplaySceneInstaller : MonoBehaviourInstaller
    {
        [SerializeField] private CameraController cameraController;
        
        public override void Register(IContainer container)
        {
            container.RegisterFromComponentOnNewPrefab(cameraController)
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
        }
    }
}