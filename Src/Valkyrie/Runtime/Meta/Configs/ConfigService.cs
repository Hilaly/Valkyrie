using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Valkyrie.Di;

namespace Configs
{
    public class ConfigService : IConfigService
    {
        private readonly Dictionary<string, IConfigData> _configData = new();
        private readonly List<IConfigLoader> _loaders = new();
        private readonly Dictionary<Type, object> _cache = new();

        public T Get<T>(string id) where T : IConfigData => (T)_configData[id];
        
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
            _cache.Clear();
            for (int i = 0; i < _loaders.Count; i++)
            {
                var loader = _loaders[i];
                var data = await loader.Load();
                foreach (var d in data) 
                    _configData[d.GetId()] = d;
            }

            foreach (var pair in _configData) 
                pair.Value.PastLoad(_configData);
        }

        public IDisposable Add(IConfigLoader loader)
        {
            _loaders.Add(loader);
            return new ActionDisposable(() => _loaders.Remove(loader));
        }
    }
}