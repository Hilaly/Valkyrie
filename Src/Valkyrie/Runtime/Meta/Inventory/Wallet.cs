namespace Meta.Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Wallet : IWallet
    {
        private readonly IInventory _inventory;

        public Wallet(IInventory inventory)
        {
            _inventory = inventory;
        }

        public long GetAmount(string id)
        {
            var c = _inventory.Get<Currency>(id);
            return c?.Amount ?? 0;
        }

        public void SetAmount(string id, long amount)
        {
            var c = _inventory.Get<Currency>(id);
            if (c != null)
                c.Amount = amount;
            else
                _inventory.Add(new Currency
                {
                    Id = id,
                    Amount = amount
                });
        }
    }
}