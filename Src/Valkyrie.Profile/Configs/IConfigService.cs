using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Configs
{
    public interface IConfigService
    {
        Task Load();
        IDisposable Add(IConfigLoader loader);
        
        T Get<T>(string id) where T : IConfigData;
        List<T> Get<T>() where T : IConfigData;
    }
}