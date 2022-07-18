using NaiveEntity.GamePrototype.EntProto;
using NaiveEntity.GamePrototype.EntProto.ViewProto;
using Valkyrie.Di;

namespace GamePrototype
{
    public class ProjectRootInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            container.Register<EntityContext>("CONFIG")
                .AsInterfacesAndSelf()
                .SingleInstance();
            container
                .Register(c => new ConfigProvider(c.Resolve<EntityContext>("CONFIG")))
                .AsInterfacesAndSelf()
                .SingleInstance();
        }
    }
}