using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IPublishMessage
    {
        void Publish<T>(T message)
            where T : IMessage, new();

        Task PublishAsync<T>(T message, CancellationToken cancellationToken = default(CancellationToken))
            where T : IMessage, new();
    }
}
