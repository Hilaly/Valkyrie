using UnityEngine;
using Valkyrie.Di;

namespace Meta
{
    public class ValkyrieMetaInstaller : MonoBehaviourInstaller
    {
        [Header("Save Data storage")]
        [SerializeField, Tooltip("Do we use local storage for persistent data")] private bool _registerLocalStorageData;
        [SerializeField] private string _localSavePath = "profile.json";

        public override void Register(IContainer container)
        {
            if (_registerLocalStorageData)
                container.Register(() => new LocalSaveDataStorage(_localSavePath))
                    .AsInterfacesAndSelf()
                    .SingleInstance();
        }
    }
}