using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;
using Utils;

namespace Valkyrie.Meta.DataSaver
{
    class ModelsProvider : IModelsProvider, ISaveDataStorage, IDisposable
    {
        private Dictionary<string, BaseModel> _models = new();
        private readonly string _localSavePath;

        private string DataPath => Path.Combine(Application.persistentDataPath, _localSavePath);

        public ModelsProvider(string localSavePath) => _localSavePath = localSavePath;

        void Log(string msg) => UnityEngine.Debug.Log($"[CORE]: {msg}");

        #region IModelsProvider

        public T Add<T>(T value) where T : BaseModel
        {
            if (Get<T>() != default && Get<T>() != value)
                throw new Exception($"Already contains model {typeof(T).Name}");
            _models.Add(typeof(T).AssemblyQualifiedName, value);
            return value;
        }

        public T Get<T>() where T : BaseModel =>
            _models.TryGetValue(typeof(T).AssemblyQualifiedName, out var r) ? (T)r : default;

        #endregion

        #region ISaveDataStorage

        public Task<bool> LoadAsync()
        {
            Log($"loading data from {DataPath}");
            if (!File.Exists(DataPath))
                return Task.FromResult(false);

            var json = File.ReadAllText(DataPath);

#if UNITY_EDITOR
            Log(json);
#endif

            _models = JsonConvert.DeserializeObject<Dictionary<string, BaseModel>>(json,
                DataExtensions.StandardJsonSettings);

            Log("data loaded");

            return Task.FromResult(true);
        }

        public async Task SaveAsync() => Save();

        private void Save() => File.WriteAllText(DataPath,
            JsonConvert.SerializeObject(_models, DataExtensions.StandardJsonSettings));

        #endregion

        public void Dispose() => Save();
    }
}