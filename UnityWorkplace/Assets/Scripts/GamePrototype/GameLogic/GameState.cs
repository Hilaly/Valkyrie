using System.Collections.Generic;
using NaiveEntity.GamePrototype.EntProto;
using NaiveEntity.GamePrototype.EntProto.ViewProto;

namespace GamePrototype.GameLogic
{
    public class GameState
    {
        public IConfig Config { get; }
        public EntityContext GameplayContext { get; }

        public IEnumerable<TownViewModel> Towns { get; }

        public GameState(EntityContext gpContext, IConfig config)
        {
            Config = config;
            GameplayContext = gpContext;

            Towns = new EntityViewsCollection<TownViewModel>(GameplayContext,
                e => e.HasComponent<TownComponent>());
        }
    }
    
    public class TownViewModel : EntityView
    {}
}