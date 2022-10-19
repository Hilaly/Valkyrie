using System.Numerics;

namespace Meta.Inventory
{
    // ReSharper disable once ClassNeverInstantiated.Global
    class Wallet : IWallet
    {
        private readonly IInventory _inventory;

        public Wallet(IInventory inventory) => _inventory = inventory;

        public BigInteger GetBigAmount(string id)
        {
            var c = _inventory.Get<Currency>(id);
            return c?.Amount ?? 0;
        }

        public void SetAmount(string id, BigInteger amount)
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

        public long GetAmount(string id) => (long)GetBigAmount(id);
        public void SetAmount(string id, long amount) => SetAmount(id, new BigInteger(amount));
    }
}