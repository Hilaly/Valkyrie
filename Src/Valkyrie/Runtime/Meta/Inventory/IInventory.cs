using System.Collections.Generic;

namespace Valkyrie.Meta.Inventory
{
    public interface IInventory
    {
        void Add(IInventoryItem item);
        T Get<T>(string id) where T : IInventoryItem;
        IEnumerable<IInventoryItem> Get();
        IEnumerable<T> Get<T>();
        void Remove(string id);
    }
}