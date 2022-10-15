using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Valkyrie.Di;

namespace Configs
{
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<string, IConfigData> _configData = new();
        private readonly List<IConfigLoader> _loaders = new();
        private readonly Dictionary<Type, object> _cache = new();

        public T Get<T>(string id) where T : IConfigData => (T)_configData[id];
        
        void Log(string msg)
        {
            Debug.Log($"[Config]: {msg}");
        }

        
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

        public async Task Load()
        {
            Log($"Loading data");
            _cache.Clear();
            for (int i = 0; i < _loaders.Count; i++)
            {
                var loader = _loaders[i];
                Log($"loader {loader.GetType().Name}");
                var data = await loader.Load();
                foreach (var d in data) 
                    _configData[d.GetId()] = d;
            }
            
            Log($"Data loaded, processing");
            
            foreach (var pair in _configData) 
                pair.Value.PastLoad(_configData);

            Log($"ready ...");
        }

        public IDisposable Add(IConfigLoader loader)
        {
            _loaders.Add(loader);
            return new ActionDisposable(() => _loaders.Remove(loader));
        }
    }
}