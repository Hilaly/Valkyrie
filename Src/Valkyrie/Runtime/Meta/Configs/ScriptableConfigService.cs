using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;

namespace Configs
{
    [CreateAssetMenu(menuName = "Valkyrie/Config")]
    public class ScriptableConfigService : ScriptableObject, IConfigService
    {
        [SerializeField] private List<ScriptableObject> serializedData = new();
        
        private readonly Dictionary<string, IConfigData> _configData = new();
        private readonly List<IConfigLoader> _loaders = new();
        private readonly Dictionary<Type, object> _cache = new();

        public T Get<T>(string id) where T : IConfigData => (T)_configData[id];
        
        void Log(string msg) => Debug.Log($"[Config]: {msg}");

        public List<T> Get<T>() where T : IConfigData
        {
            if (!_cache.TryGetValue(typeof(T), out var list))
                _cache.Add(typeof(T),
                    list = _configData
                        .Where(x => x.Value is T)
                        .Select(pair => (T)pair.Value)
                        .ToList());
            return (List<T>)list;
        }

        public IDisposable Add(IConfigLoader loader)
        {
            _loaders.Add(loader);
            return new ActionDisposable(() => _loaders.Remove(loader));
        }
        
        public async Task Load()
        {
            Log($"Loading data");
            _cache.Clear();
            
            Integrate(serializedData.OfType<IConfigData>());
            
            for (int i = 0; i < _loaders.Count; i++)
            {
                var loader = _loaders[i];
                Log($"loader {loader.GetType().Name}");
                
                var data = await loader.Load();
                
                Integrate(data);
            }
            
            Log($"Data loaded, processing");
            
            foreach (var pair in _configData) 
                pair.Value.PastLoad(_configData);

            Log($"ready ...");
        }

        void Integrate(IEnumerable<IConfigData> data)
        {
            foreach (var d in data) 
                _configData[d.GetId()] = d;
        }

        public T Create<T>() where T : ScriptableObject, IConfigData
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
                if(this == asset)
                    continue;
                if(this.serializedData.Contains(asset))
                    continue;
                UnityEditor.AssetDatabase.RemoveObjectFromAsset(asset);
                UnityEditor.AssetDatabase.Refresh();
            }
            
            UnityEditor.AssetDatabase.ImportAsset(UnityEditor.AssetDatabase.GetAssetPath(this));
#endif
        }
    }
}