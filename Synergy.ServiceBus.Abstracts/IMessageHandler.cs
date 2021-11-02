using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IMessageHandler<in T> : IMessageHandler
        where T : IMessage
    {
        void Handle(T message);

        Task HandleAsync(T message, CancellationToken cancellationToken);
    }

#pragma warning disable CA1040 // Avoid empty interfaces
    public interface IMessageHandler
#pragma warning restore CA1040 // Avoid empty interfaces
    {
    }
}
