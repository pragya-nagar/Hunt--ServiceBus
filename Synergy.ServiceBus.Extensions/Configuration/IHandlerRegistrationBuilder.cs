using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public interface IHandlerRegistrationBuilder
    {
        IHandlerRegistrationBuilder Subscribe<THandler, TMessage>(HandleOptions handleOptions = null)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage;

        IHandlerRegistrationBuilder SubscribeToServiceEvent<THandler, TMessage>()
            where THandler : class, IServiceHandler<TMessage>
            where TMessage : class, IServiceMessage;

        IHandlerRegistrationBuilder SubscribeWithOptions<THandler, TMessage>(TimeSpan timeout, bool disableParallel = false)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage;
    }
}