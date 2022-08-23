using UnityEngine.Assertions;

namespace Meta.Inventory
{
    public abstract class ItemWrapper
    {
        public Item Item { get; }

        string GetTypeId() => GetType().Name;
        
        protected ItemWrapper(Item item)
        {
            Assert.AreEqual(GetTypeId(), item.TypeId);
            Item = item;
        }

        protected ItemWrapper(string id)
        {
            Item = new Item() { Id = id, TypeId = GetTypeId() };
        }
    }
}