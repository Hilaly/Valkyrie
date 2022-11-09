using Configs;
using Meta.Commands;
using Meta.PlayerInfo;
using UnityEngine;
using Valkyrie.Di;

namespace Meta
{
    public class ValkyrieMetaInstaller : MonoBehaviourInstaller
    {
        [Header("Save Data storage")]
        [SerializeField, Tooltip("Do we use local storage for persistent data")] private bool _registerLocalStorageData;
        [SerializeField] private string _localSavePath = "profile.json";

        [Header("Use Valkyrie Configs")] 
        [SerializeField, Tooltip("Scriptable Instance of Config")]
        private ScriptableConfigService configService;

        [SerializeField, Tooltip("Do we use standard json configs")]
        private bool useJsonConfig = true;

        [Header("Use commands")] [SerializeField, Tooltip("Do we need commands handling")]
        private bool useCommands = true;

        public override void Register(IContainer container)
        {
            if (configService != null)
            {
                container.Register(configService).AsInterfacesAndSelf();
                
                if (useJsonConfig)
                    container.Register<JsonConfigLoader>()
                        .AsInterfacesAndSelf()
                        .SingleInstance()
                        .NonLazy();
            }

            container.Register<PlayerInfoProvider>()
                .AsInterfacesAndSelf()
                .SingleInstance();
            container.Register<Inventory.InventoryProvider>()
                .AsInterfacesAndSelf()
                .SingleInstance();
            container.Register<Inventory.Wallet>()
                .AsInterfacesAndSelf()
                .SingleInstance();
            
            if (_registerLocalStorageData)
                container.Register(() => new LocalSaveDataStorage(_localSavePath))
                    .AsInterfacesAndSelf()
                    .SingleInstance();

            if (useCommands)
            {
                container.Register<CommandsProcessor>()
                    .AsInterfacesAndSelf()
                    .SingleInstance();
                
                container.Register<CommandArgsResolver>()
                    .AsInterfacesAndSelf()
                    .SingleInstance();
            }
        }
    }
}