using System;
using System.Threading;
using System.Threading.Tasks;

namespace Valkyrie.MVVM
{
    public interface ITaskScheduler
    {
        Task RunOnMainThread(Action work, CancellationToken cancellationToken);
    }
}