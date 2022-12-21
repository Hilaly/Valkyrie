using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;
using Valkyrie.Di;

namespace Valkyrie.Meta.DataSaver
{
    class LocalSaveDataStorage : ISaveDataStorage, IDisposable
    {
        private readonly string _localPath;
        private readonly HashSet<ISaveDataProvider> _dataProviders = new();
        private JObject _saveData;

        void Log(string msg)
        {
            Debug.Log($"[SAVE]: {msg}");
        }
        
        private string DataPath => Path.Combine(Application.persistentDataPath, _localPath);

        public LocalSaveDataStorage(string localPath)
        {
            _localPath = localPath;
            
            Log("storage created");
        }
        
        public async Task<bool> LoadAsync()
        {
            Log($"loading data from {DataPath}");
            if(!File.Exists(DataPath))
                return false;
            
            var json = await File.ReadAllTextAsync(DataPath);
            
#if UNITY_EDITOR
            Log(json);
#endif
            
            _saveData = JObject.Parse(json);
            foreach (var dataProvider in _dataProviders) 
                ParseEntry(dataProvider);
            
            Log("data loaded");

            return true;
        }

        void ParseEntry(ISaveDataProvider dataProvider)
        {
            var key = dataProvider.Key;
            
            if (!_saveData.ContainsKey(key)) 
                return;
            
            var jsonValue = _saveData.GetValue(key).Value<string>();
            Log($"parsing '{key}': {jsonValue}");

            dataProvider.SetData(jsonValue);
        }

        public async Task SaveAsync()
        {
            Log($"saving data to {DataPath}");
            _saveData ??= new JObject();
            
            foreach (var dataProvider in _dataProviders) 
                _saveData[dataProvider.Key] = dataProvider.GetData();

            var json = _saveData.ToString(Formatting.Indented);
            await File.WriteAllTextAsync(DataPath, json);
            
#if UNITY_EDITOR
            Log(json);
#endif
            
            Log("data saved");
        }

        public IDisposable RegisterProvider(ISaveDataProvider provider)
        {
            if (_dataProviders.Add(provider))
            {
                if(_saveData != null)
                    ParseEntry(provider);
                return new ActionDisposable(() => _dataProviders.Remove(provider));
            }
            throw new Exception("Try to register ISaveDataProvider twice");
        }

        public void Dispose()
        {
            SaveAsync();
            
            Log("storage disposed");
        }
    }
}