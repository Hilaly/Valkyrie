namespace Meta.Inventory
{
    public class Currency : ItemWrapper
    {
        public Currency(Item item) : base(item)
        {
        }

        public Currency(string id) : base(id)
        {
        }

        public Currency()
        {
        }

        public long Amount
        {
            get => Item.Get(nameof(Amount), 0L);
            set => Item.Set(nameof(Amount), value);
        }
    }
}