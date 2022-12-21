using UnityEngine;
using Utils;
using Valkyrie.Di;

namespace Valkyrie.Entities
{
    public class EntitiesConfigInstaller : MonoBehaviourInstaller
    {
        [SerializeField] private bool _registerAllComponents = true;
        
        public override void Register(IContainer container)
        {
            container.Register<EntitiesConfigService>()
                .AsInterfacesAndSelf()
                .OnActivation(inst =>
                {
                    if (!_registerAllComponents) 
                        return;
                    
                    foreach (var componentType in typeof(IComponent).GetAllSubTypes(x => x.IsClass && !x.IsAbstract))
                        inst.Instance.RegisterComponent(componentType);
                })
                .SingleInstance()
                .NonLazy();
        }
    }
}