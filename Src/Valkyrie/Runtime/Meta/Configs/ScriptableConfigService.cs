using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Valkyrie.Meta.Configs
{
    [CreateAssetMenu(menuName = "Valkyrie/Config")]
    public class ScriptableConfigService : ScriptableObject, IConfigService
    {
        [SerializeField] private List<ScriptableConfigData> serializedData = new();

        private readonly DataStorage<IConfigData> _dataStorage = new();

        void Log(string msg) => Debug.Log($"[Config]: {msg}");

        public TC Get<TC>(string id) where TC : IConfigData => _dataStorage.Get<TC>(id);

        public IReadOnlyList<TC> Get<TC>() where TC : IConfigData => _dataStorage.Get<TC>();

        public Task Load()
        {
            Log($"Loading data");

            _dataStorage.Clear();

            foreach (var o in serializedData)
            {
                _dataStorage.Add(o, o.GetId());
            }

            Log($"Data loaded, processing");

            foreach (var pair in serializedData)
                pair.PastLoad(_dataStorage.Dictionary);

            Log($"ready ...");

            return Task.CompletedTask;
        }

        public T Create<T>() where T : ScriptableConfigData, IConfigData
        {
            var r = CreateInstance<T>();
            r.name = Guid.NewGuid().ToString();
            serializedData.Add(r);

#if UNITY_EDITOR
            UnityEditor.AssetDatabase.AddObjectToAsset(r, this);
            UnityEditor.AssetDatabase.Refresh();

            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(r));
#endif

            return r;
        }

        public void Refresh()
        {
#if UNITY_EDITOR
            var assets = UnityEditor.AssetDatabase.LoadAllAssetsAtPath(UnityEditor.AssetDatabase.GetAssetPath(this));
            foreach (var asset in assets)
            {
                if (asset is ScriptableConfigData cd)
                    cd.name = cd.id;
                if (this == asset)
                    continue;
                if (this.serializedData.Contains(asset))
                    continue;
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(asset);
                UnityEditor.AssetDatabase.Refresh();
            }

            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
#endif
        }
    }
}