using System.Numerics;

namespace Valkyrie.Meta.Models
{
    public interface IWallet
    {
        BigInteger GetBigAmount(string id);
        void SetAmount(string id, BigInteger amount);
        
        long GetAmount(string id);
        void SetAmount(string id, long amount);
    }
}