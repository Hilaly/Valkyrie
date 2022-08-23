using System.Collections.Generic;
using Newtonsoft.Json;

namespace Meta.Inventory
{
    class InventoryProvider : ISaveDataProvider, IInventory
    {
        private readonly Dictionary<string, Item> _items = new Dictionary<string, Item>();

        public string Key => "INTERNAL_INVENTORY";
        
        public string GetData()
        {
            return JsonConvert.SerializeObject(_items);
        }

        public void SetData(string jsonData)
        {
            var temp = JsonConvert.DeserializeObject<Dictionary<string, Item>>(jsonData);
            _items.Clear();
            foreach (var item in temp) 
                _items.Add(item.Key, item.Value);
        }

        public void Add(Item item) => _items[item.Id] = item;

        public Item Get(string id) => _items.TryGetValue(id, out var r) ? r : default;

        public IEnumerable<Item> GetAll() => _items.Values;

        public void Remove(string id) => _items.Remove(id);
    }
}