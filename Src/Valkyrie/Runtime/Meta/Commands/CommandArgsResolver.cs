using Configs;
using Meta.Inventory;
using Meta.PlayerInfo;
using Valkyrie.Di;

namespace Meta.Commands
{
    class CommandArgsResolver
    {
        [InjectOptional] private IInventory _inventory;
        [InjectOptional] private IWallet _wallet;
        [InjectOptional] private IPlayerInfoProvider _infoProvider;
        [InjectOptional] private IConfigService _configService;

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