using Configs;
using Meta.Inventory;
using Meta.PlayerInfo;

namespace Meta.Commands
{
    public class CommandContext
    {
        public IPlayerInfoProvider PlayerInfoProvider;
        public IInventory Inventory;
        public IWallet Wallet;
        public IConfigService Config;
    }
}