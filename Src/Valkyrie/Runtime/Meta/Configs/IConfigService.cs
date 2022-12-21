using System.Threading.Tasks;

namespace Valkyrie.Meta.Configs
{
    public interface IConfigService : IDataStorage<IConfigData>
    {
        Task Load();
    }
}