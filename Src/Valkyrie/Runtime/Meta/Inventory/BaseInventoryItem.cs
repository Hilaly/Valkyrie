namespace Valkyrie.Meta.Inventory
{
    public abstract class BaseInventoryItem : IInventoryItem
    {
        public string Id { get; set; }
        public virtual string TypeId => GetType().Name;
    }
}