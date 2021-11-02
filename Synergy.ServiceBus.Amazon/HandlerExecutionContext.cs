using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Amazon
{
#pragma warning disable CA1812
    internal class HandlerExecutionContext<TMessage, TMessageHandler> : IHandlerExecutionContext
        where TMessage : IMessage
        where TMessageHandler : IMessageHandler<TMessage>
    {
        private readonly IHandlerScope<TMessage, TMessageHandler> _handlerScope;

        public HandlerExecutionContext(MessageContext messageContext, IHandlerScope<TMessage, TMessageHandler> handler)
        {
            this.MessageContext = messageContext;

            this._handlerScope = handler;
        }

        public MessageContext MessageContext { get; }

        public HandleOptions Options => this._handlerScope.HandleOptions;

        public DateTime? HandlerStartedTimestamp { get; set; }

        public IMessageHandler Handler => this._handlerScope.Handler;

        public void Dispose()
        {
            this._handlerScope.Dispose();

            this.MessageContext.DetachHandler(this);
        }

        public Task ExecuteAsync(dynamic message, CancellationToken cancellationToken)
        {
            if (this._handlerScope.Handler == null)
            {
                throw new ApplicationException($"Handler for {message.GetType()} was not found.");
            }

            dynamic handler = this._handlerScope.Handler;

            return handler.HandleAsync(message, cancellationToken);
        }
    }
#pragma warning restore CA1812
}