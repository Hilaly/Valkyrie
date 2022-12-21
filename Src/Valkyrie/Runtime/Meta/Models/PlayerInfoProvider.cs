using Valkyrie.Meta.DataSaver;

namespace Valkyrie.Meta.Models
{
    class PlayerInfoProvider : DefaultModelProvider<PlayerInfo>, IPlayerInfoProvider
    {
        public PlayerInfo Info => Model;

        public PlayerInfoProvider(IModelsProvider modelsProvider) : base(modelsProvider)
        {
        }
    }
}