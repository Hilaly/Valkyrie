using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Playground;

namespace Prototype.TryEvents
{
    class GameObjectContextWorker : MonoBehaviour, IHasId
    {
        [Inject] private IRootContext _rootContext;

        private void Awake() => _rootContext.SubscribeAllEvents(gameObject);

        public string Id => name;
    }
}