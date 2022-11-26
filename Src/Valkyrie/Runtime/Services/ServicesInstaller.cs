using System.Collections.Generic;
using Valkyrie.Di;
using Valkyrie.UserInput;

namespace Services
{
    public class ServicesInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            container.RegisterSingleInstance<PathFinderService>();
            container.RegisterSingleInstance<InputService>();
        }
    }
}