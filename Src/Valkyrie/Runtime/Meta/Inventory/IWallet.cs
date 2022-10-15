namespace Meta.Inventory
{
    public interface IWallet
    {
        long GetAmount(string id);
        void SetAmount(string id, long amount);
    }
}