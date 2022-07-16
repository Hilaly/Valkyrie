using Valkyrie.Di;

namespace GamePrototype
{
    public class ProjectRootInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            container.Register<EntityContext>()
                .AsInterfacesAndSelf()
                .SingleInstance();
        }
    }
}