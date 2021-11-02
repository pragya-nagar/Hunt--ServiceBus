using System;
using Microsoft.Extensions.DependencyInjection;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public sealed class HandlerScope<TMessage, THandler> : IHandlerScope<TMessage, THandler>
        where TMessage : IMessage
        where THandler : IMessageHandler<TMessage>
    {
        private readonly IServiceScope _serviceScope;

        public HandlerScope(IServiceProvider serviceProvider, HandleOptions handleOptions = null)
        {
            this.HandleOptions = handleOptions;

            this._serviceScope = serviceProvider.CreateScope();
        }

        public HandleOptions HandleOptions { get; }

        public THandler Handler => this._serviceScope.ServiceProvider.GetRequiredService<THandler>();

        public void Dispose()
        {
            this._serviceScope.Dispose();
        }
    }
}