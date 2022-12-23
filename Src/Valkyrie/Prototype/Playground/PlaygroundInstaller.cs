using System;
using UnityEngine;
using Utils;
using Valkyrie.Di;
using Valkyrie.Ecs;
using Valkyrie.Utils.Pool;

namespace Valkyrie.Playground
{
    class PlaygroundInstaller : MonoBehaviourInstaller
    {
        [SerializeField] private EntitiesDatabase entitiesDatabase;
        [SerializeField] private bool internalSimulationSettings = true;

        public override void Register(IContainer container)
        {
            if (internalSimulationSettings)
                container.Register<SimulationSettings>()
                    .AsInterfacesAndSelf()
                    .OnActivation(x => x.Instance.IsSimulationPaused = true)
                    .SingleInstance();

            if (entitiesDatabase != null)
                container.Register(entitiesDatabase)
                    .AsInterfacesAndSelf();

            typeof(IFeature)
                .GetAllSubTypes(x => x.IsClass && !x.IsAbstract)
                .ConvertAll(x => (IFeature)Activator.CreateInstance(x))
                .ForEach(x => container.RegisterLibrary(x));

            container.RegisterSingleInstance<GameState>();

            container.RegisterSingleInstance<ObjectsPool>();

            container.RegisterFromNewComponentOnNewGameObject<World>()
                .AsInterfacesAndSelf()
                .SingleInstance()
                .NonLazy();
        }
    }
}