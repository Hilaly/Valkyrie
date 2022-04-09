using UnityEngine;

namespace Valkyrie.Di
{
    public abstract class MonoBehaviourInstaller : MonoBehaviour, ILibrary
    {
        public abstract void Register(IContainer container);
    }
}