using System.Collections.Generic;

namespace Valkyrie.Meta.Models
{
    public interface IInventory
    {
        void Add(IInventoryItem item);
        T Get<T>(string id) where T : IInventoryItem;
        IReadOnlyList<IInventoryItem> Get();
        IReadOnlyList<T> Get<T>() where T : IInventoryItem;
        void Remove(string id);
    }
}