using Valkyrie.Di;
using Valkyrie.Playground.Features;

namespace Valkyrie.Playground.Triggers
{
    public class PlayerTriggerComponent : FilterByExistComponentTriggerComponent<IPlayerComponent>
    {
        [Inject] private GameState _world;
        
        protected override void OnEnter(IEntity e)
        {
            _world.SendEvent(new PlayerEnterTriggerEvent { PlayerEntity = e, TriggerEntity = Entity });
        }

        protected override void OnExit(IEntity e)
        {
            _world.SendEvent(new PlayerExitTriggerEvent { PlayerEntity = e, TriggerEntity = Entity });
        }
    }
}