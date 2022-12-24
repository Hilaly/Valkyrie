using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie
{
    public class FullLogicInstaller : MonoBehaviourInstaller
    {
        public override void Register(IContainer container)
        {
            Debug.LogWarning($"Not implemented installing logic context");

            container.Register<GrammarProvider>().AsInterfacesAndSelf().SingleInstance();
        }
    }
}