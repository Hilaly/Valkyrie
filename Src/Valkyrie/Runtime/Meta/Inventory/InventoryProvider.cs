using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class InventoryProvider : ISaveDataProvider, IInventory, IDisposable
    {
        private readonly Dictionary<string, IInventoryItem> _items = new();
        private readonly IDisposable _disposable;

        private JsonSerializerSettings SerSettings { get; } = new() { TypeNameHandling = TypeNameHandling.All };

        public InventoryProvider(ISaveDataStorage dataStorage) => _disposable = dataStorage.RegisterProvider(this);

        #region IDisposable

        public void Dispose()
        {
            _disposable?.Dispose();
        }

        #endregion

        #region ISaveDataProvider

        string ISaveDataProvider.Key => "INTERNAL_INVENTORY";

        string ISaveDataProvider.GetData() => JsonConvert.SerializeObject(_items.Values.ToArray(), SerSettings);

        void ISaveDataProvider.SetData(string jsonData)
        {
            _items.Clear();
            var temp = JsonConvert.DeserializeObject<IInventoryItem[]>(jsonData, SerSettings);
            if(temp == null)
                return;
            for (var index = 0; index < temp.Length; index++)
            {
                var item = temp[index];
                _items.Add(item.Id, item);
            }
        }

        #endregion

        #region IInventory

        public void Add(IInventoryItem item) => _items[item.Id] = item;

        public T Get<T>(string id) where T : IInventoryItem => _items.TryGetValue(id, out var item) && item is T result ? result : default;

        public IEnumerable<IInventoryItem> Get() => _items.Values;

        public IEnumerable<T> Get<T>() => Get().OfType<T>();

        public void Remove(string id) => _items.Remove(id);

        #endregion
    }
}