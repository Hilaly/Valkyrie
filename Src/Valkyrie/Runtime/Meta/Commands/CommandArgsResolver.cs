using Configs;
using Meta.Inventory;
using Meta.PlayerInfo;

namespace Meta.Commands
{
    class CommandArgsResolver
    {
        private IInventory _inventory;
        private IWallet _wallet;
        private IPlayerInfoProvider _infoProvider;
        private IConfigService _configService;

        public CommandArgsResolver(IInventory inventory, IWallet wallet, IPlayerInfoProvider infoProvider, IConfigService configService)
        {
            _inventory = inventory;
            _wallet = wallet;
            _infoProvider = infoProvider;
            _configService = configService;
        }

        public CommandContext Create()
        {
            return new CommandContext()
            {
                Inventory = _inventory,
                Wallet = _wallet,
                PlayerInfoProvider = _infoProvider,
                Config = _configService
            };
        }
    }
}