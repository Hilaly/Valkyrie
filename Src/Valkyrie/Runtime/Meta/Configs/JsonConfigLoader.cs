using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace Configs
{
    class JsonConfigLoader : IConfigLoader, IDisposable
    {
        private readonly JsonSerializerSettings _jsonSerializerSettings = new()
        {
            TypeNameHandling = TypeNameHandling.Auto
        };

        private string _resourceName = "config";
        private readonly IDisposable _disposable;

        public JsonConfigLoader(IConfigService service)
        {
            _disposable = service.Add(this);
        }

        Task<IEnumerable<IConfigData>> IConfigLoader.Load()
        {
            var resource = Resources.Load<TextAsset>(_resourceName);
            if (resource == null)
            {
                Debug.LogWarning($"[JsonConfig]: Couldn't find resource {_resourceName}");
                return Task.FromResult(Enumerable.Empty<IConfigData>());
            }

            Debug.Log($"[JsonConfig]: Loading config data from {_resourceName}");
            var list = JsonConvert.DeserializeObject<List<IConfigData>>(resource.text, _jsonSerializerSettings);
            Debug.Log($"[JsonConfig]: {list.Count} data loaded");
            return Task.FromResult<IEnumerable<IConfigData>>(list);
        }

        void IDisposable.Dispose()
        {
            _disposable?.Dispose();
        }
    }
}