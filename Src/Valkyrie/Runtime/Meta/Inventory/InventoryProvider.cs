using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace Meta.Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class InventoryProvider : ISaveDataProvider, IInventory
    {
        private readonly Dictionary<string, IInventoryItem> _items = new();

        public string Key => "INTERNAL_INVENTORY";

        public string GetData()
        {
            return JsonConvert.SerializeObject(_items);
        }

        public void SetData(string jsonData)
        {
            var temp = JsonConvert.DeserializeObject<Dictionary<string, IInventoryItem>>(jsonData);
            _items.Clear();
            foreach (var item in temp)
                _items.Add(item.Key, item.Value);
        }

        public void Add(IInventoryItem item)
        {
            throw new System.NotImplementedException();
        }

        public T Get<T>(string id) where T : IInventoryItem
        {
            if (_items.TryGetValue(id, out var item) && item is T result)
                return result;
            return default;
        }

        public IEnumerable<IInventoryItem> Get() => _items.Values;

        public IEnumerable<T> Get<T>() => Get().OfType<T>();

        public void Remove(string id) => _items.Remove(id);
    }
}