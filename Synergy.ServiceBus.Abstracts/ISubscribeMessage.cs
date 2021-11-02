using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts
{
    public interface ISubscribeMessage
    {
        void Subscribe<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>;

        Task SubscribeAsync<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory, CancellationToken cancellationToken = default(CancellationToken))
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>;
    }
}
