using System.Threading.Tasks;
using UnityEngine;

namespace Valkyrie.Meta.Configs
{
    class JsonConfigService : DataStorage<IConfigData>, IConfigService
    {
        private readonly TextAsset _dataSource;

        public JsonConfigService(TextAsset dataSource)
        {
            _dataSource = dataSource;
        }

        public Task Load()
        {
            Load(_dataSource.text);
            return Task.CompletedTask;
        }
    }
}