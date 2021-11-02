using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Extensions.Progress
{
    public interface IProgressPublisher
    {
        Task PostProgressAsync(int progress, CancellationToken cancellationToken);

        Task IncrementProgressAsync(int increment, CancellationToken cancellationToken);

        IDisposable CreateChildScope(int scopeSize);
    }
}