using System.Collections.Generic;

namespace Meta.Inventory
{
    public interface IInventory
    {
        void Add(Item item);
        Item Get(string id);
        IEnumerable<Item> GetAll();
        void Remove(string id);
    }
}