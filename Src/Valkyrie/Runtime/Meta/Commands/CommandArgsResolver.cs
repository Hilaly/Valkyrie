using Valkyrie.Di;
using Valkyrie.Meta.Configs;
using Valkyrie.Meta.Models;

namespace Valkyrie.Meta.Commands
{
    class CommandArgsResolver
    {
        [InjectOptional] private IInventory _inventory;
        [InjectOptional] private IWallet _wallet;
        [InjectOptional] private IPlayerInfoProvider _infoProvider;
        [InjectOptional] private IConfigService _configService;

        public CommandContext Create() =>
            new(_infoProvider, _inventory, _wallet, _configService);
    }
}