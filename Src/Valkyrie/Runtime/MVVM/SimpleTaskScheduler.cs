using System;
using System.Threading;
using System.Threading.Tasks;

namespace Valkyrie.MVVM
{
    public class SimpleTaskScheduler : ITaskScheduler
    {
        public async Task RunOnMainThread(Action work, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return; 
            await AsyncExtension.WaitForEndOfFrame();
            if (cancellationToken.IsCancellationRequested)
                return; 
            work();
        }
    }
}