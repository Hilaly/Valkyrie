namespace Meta.Inventory
{
    class Wallet : IWallet
    {
        private readonly IInventory _inventory;

        public Wallet(IInventory inventory)
        {
            _inventory = inventory;
        }

        public long GetAmount(string id)
        {
            var c = _inventory.Wrap<Currency>(id);
            return c?.Amount ?? 0;
        }

        public void SetAmount(string id, long amount)
        {
            var c = _inventory.Wrap<Currency>(id);
            if (c == null)
                _inventory.Add(new Currency(id) { Amount = amount });
            else
                c.Amount = amount;
        }
    }
}