using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configs
{
    public interface IConfigLoader
    {
        Task<IEnumerable<IConfigData>> Load();
    }
}