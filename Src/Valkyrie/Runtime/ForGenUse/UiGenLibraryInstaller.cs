using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
    public class UiGenLibraryInstaller : MonoBehaviourInstaller
    {
        [SerializeField] private WindowManager windowManager;
        [SerializeField] private PopupsManager popupsManager;
        
        public override void Register(IContainer container)
        {
            container.RegisterSingleInstance<EventSystem>();
            container.RegisterSingleInstance<CommandsInterpreter>();
            container.Register(windowManager).AsInterfacesAndSelf();
            container.Register(popupsManager).AsInterfacesAndSelf();
            container.Register<UiCommands>().AsInterfacesAndSelf().SingleInstance().NonLazy();
        }
    }
}