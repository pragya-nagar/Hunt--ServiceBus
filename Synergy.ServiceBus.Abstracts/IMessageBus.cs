using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IMessageBus : ISubscribeMessage, IPublishMessage
    {
        Task StartListeningAsync(CancellationToken cancellationToken = default(CancellationToken));
    }
}
