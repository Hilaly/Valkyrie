using Valkyrie.Di;

namespace Hilaly.Services
{
    public class ServicesInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            container.RegisterSingleInstance<PathFinderService>();
        }
    }
}