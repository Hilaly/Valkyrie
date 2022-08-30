using Valkyrie.Di;

namespace Valkyrie.Entities
{
    public class EntitiesConfigInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            container.Register<EntitiesConfigService>()
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
        }
    }
}