using Valkyrie.Di;

namespace Prototype.TryEvents
{
    public class UnityEventsLibrary : ILibrary
    {
        public void Register(IContainer container) => container.RegisterSingleInstance<RootContext>();
    }
}