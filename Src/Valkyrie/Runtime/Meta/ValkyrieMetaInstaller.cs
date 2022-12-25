using UnityEngine;
using Valkyrie.Di;
using Valkyrie.Meta.Commands;
using Valkyrie.Meta.Configs;
using Valkyrie.Meta.DataSaver;
using Valkyrie.Meta.Models;
using Valkyrie.UI;

namespace Valkyrie.Meta
{
    public class ValkyrieMetaInstaller : MonoBehaviourInstaller
    {
        [Header("Data Storage")] [SerializeField, Tooltip("Do we use local storage for persistent data")]
        private bool registerLocalStorageData;

        [SerializeField] private string localSavePath = "profile.json";

        [Header("Configs")] [SerializeField, Tooltip("Scriptable Instance of Config (Optional)")]
        private ScriptableConfigService configService;

        [SerializeField, Tooltip("Do we use standard json configs (Optional)")]
        private TextAsset jsonConfig;

        [Header("Use commands")] [SerializeField, Tooltip("Do we need commands handling")]
        private bool useCommands = true;

        public override void Register(IContainer container)
        {
            if (jsonConfig != default)
                container.Register(new JsonConfigService(jsonConfig))
                    .AsInterfacesAndSelf();
            else if (configService != null)
                container.Register(configService).AsInterfacesAndSelf();
            else
                Debug.LogWarning($"[CORE]: config service isn't registered");

            if (registerLocalStorageData)
            {
                container.Register(() => new ModelsProvider(localSavePath))
                    .AsInterfacesAndSelf()
                    .SingleInstance();

                container.RegisterSingleInstance<PlayerInfoProvider>();
                container.RegisterSingleInstance<InventoryProvider>();
                container.RegisterSingleInstance<Wallet>();
            }

            if (useCommands)
            {
                container.RegisterSingleInstance<CommandArgsResolver>();
                container.RegisterSingleInstance<CommandsProcessor>();
            }

            container.RegisterSingleInstance<UiManager>();
        }
    }
}