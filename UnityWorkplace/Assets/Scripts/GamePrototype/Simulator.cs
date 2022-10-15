using GamePrototype.GameLogic;
using NaiveEntity.GamePrototype.EntProto;
using UnityEngine;
using Valkyrie.Di;

namespace GamePrototype
{
    public class Simulator : MonoBehaviour
    {
        [Inject] private GameState _context;

        private void Update()
        {
            var ev = new UpdateEvent(Time.deltaTime, Time.time);

            foreach (var entity in _context.GameplayContext.Get())
                entity.PropagateEvent(ev);
        }
    }
}