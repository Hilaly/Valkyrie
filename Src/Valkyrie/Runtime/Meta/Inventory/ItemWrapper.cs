using UnityEngine.Assertions;

namespace Meta.Inventory
{
    public abstract class ItemWrapper
    {
        public Item Item { get; set; }

        public string Id => Item.Id;
        public string TypeId => Item.TypeId;
        
        protected virtual string GetTypeId() => GetType().Name;

        protected ItemWrapper()
        {
        }

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