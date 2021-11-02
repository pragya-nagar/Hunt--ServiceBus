using System;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IHandlerScope<out TMessage, out THandler> : IHandlerScope
        where TMessage : IMessage
        where THandler : IMessageHandler<TMessage>
    {
        THandler Handler { get; }
    }

    public interface IHandlerScope : IDisposable
    {
        HandleOptions HandleOptions { get; }
    }
}