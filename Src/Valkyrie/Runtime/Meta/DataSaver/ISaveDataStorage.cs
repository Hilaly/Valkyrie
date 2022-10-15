using System;
using System.Threading.Tasks;

namespace Meta
{
    public interface ISaveDataStorage
    {
        Task<bool> LoadAsync();
        Task SaveAsync();

        IDisposable RegisterProvider(ISaveDataProvider provider);
    }
}