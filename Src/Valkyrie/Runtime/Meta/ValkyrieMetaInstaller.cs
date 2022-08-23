using Configs;
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

        [Header("Use Valkyrie Configs")] [SerializeField, Tooltip("Do we use configs")]
        private bool useConfigs = true;

        public override void Register(IContainer container)
        {
            if (useConfigs)
                container.Register<ConfigService>()
                    .AsInterfacesAndSelf()
                    .SingleInstance();
            
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
        }
    }
}