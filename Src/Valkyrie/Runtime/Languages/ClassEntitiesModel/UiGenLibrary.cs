using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
    public class UiGenLibrary : MonoBehaviourInstaller
    {
        [SerializeField] private WindowManager windowManager;
        [SerializeField] private PopupsManager popupsManager;
        
        public override void Register(IContainer container)
        {
            container.Register(windowManager).AsInterfacesAndSelf();
            container.Register(popupsManager).AsInterfacesAndSelf();
        }
    }
}