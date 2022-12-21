using Configs;
using Meta.Inventory;
using Meta.PlayerInfo;

namespace Valkyrie.Meta.Commands
{
    public class CommandContext
    {
        public readonly IPlayerInfoProvider PlayerInfoProvider;
        public readonly IInventory Inventory;
        public readonly IWallet Wallet;
        public readonly IConfigService Config;

        public CommandContext(IPlayerInfoProvider playerInfoProvider, IInventory inventory, IWallet wallet, IConfigService config)
        {
            PlayerInfoProvider = playerInfoProvider;
            Inventory = inventory;
            Wallet = wallet;
            Config = config;
        }
    }
}