using System.Collections.Generic;
using NaiveEntity.GamePrototype.EntProto;
using NaiveEntity.GamePrototype.EntProto.ViewProto;
using UnityEngine;
using Valkyrie.MVVM;

namespace GamePrototype.GameLogic
{
    public class GameState
    {
        public IConfig Config { get; }
        public EntityContext GameplayContext { get; }

        public IEnumerable<TownViewModel> Towns { get; }
        public IEnumerable<BuildingViewModel> Buildings { get; }

        public GameState(EntityContext gpContext, IConfig config)
        {
            Config = config;
            GameplayContext = gpContext;

            Towns = new EntityViewsCollection<TownViewModel>(GameplayContext,
                e => e.HasComponent<TownComponent>());
            Buildings = new EntityViewsCollection<BuildingViewModel>(GameplayContext,
                e => e.HasComponent<BuildingComponent>());
        }
    }
    
    [Binding]
    public class TownViewModel : EntityView
    {
        [Binding] public Vector3 Position => Entity.GetComponent<PositionComponent>().Value;
    }

    [Binding]
    public class BuildingViewModel : EntityView
    {
        [Binding] public Vector3 Position => Entity.GetComponent<PositionComponent>().Value;
    }
}