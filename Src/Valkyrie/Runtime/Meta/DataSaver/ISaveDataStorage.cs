using System;
using System.Threading.Tasks;

namespace Valkyrie.Meta.DataSaver
{
    public interface ISaveDataStorage
    {
        Task<bool> LoadAsync();
        Task SaveAsync();

        IDisposable RegisterProvider(ISaveDataProvider provider);
    }
}