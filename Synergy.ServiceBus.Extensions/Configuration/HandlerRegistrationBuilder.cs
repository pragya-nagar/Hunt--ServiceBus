using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public class HandlerRegistrationBuilder<TOptions> : IHandlerRegistrationBuilder
        where TOptions : class
    {
        private readonly IServiceCollection _serviceCollection;

        private readonly Subscriptions _subscriptions = new Subscriptions();

        public HandlerRegistrationBuilder(IServiceCollection serviceCollection)
        {
            this._serviceCollection = serviceCollection;
        }

        public HandlerRegistrationBuilder<TOptions> Configure(Action<TOptions> configureAction)
        {
            this._serviceCollection.Configure(configureAction);

            return this;
        }

        public HandlerRegistrationBuilder<TOptions> Configure(IConfiguration config, string configSection)
        {
            this._serviceCollection.Configure<TOptions>(x => config.Bind(configSection, x));

            return this;
        }

        public IHandlerRegistrationBuilder Subscribe<THandler, TMessage>(HandleOptions handleOptions = null)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage
        {
            this._serviceCollection.AddTransient<IMessageHandler<TMessage>, THandler>();
            this._serviceCollection.AddTransient<THandler>();

            this._subscriptions.Registrations.Add(((messageBus, provider, options, token) => messageBus.SubscribeAsync<TMessage, THandler>(provider, handleOptions, token), handleOptions));

            return this;
        }

        public IHandlerRegistrationBuilder SubscribeWithOptions<THandler, TMessage>(TimeSpan timeout, bool disableParallel = false)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage
        {
            this.Subscribe<THandler, TMessage>(new HandleOptions
            {
                ExecutionTimeout = timeout,
                DisableParallelProcessing = disableParallel,
            });

            return this;
        }

        public IHandlerRegistrationBuilder SubscribeToServiceEvent<THandler, TMessage>()
            where THandler : class, IServiceHandler<TMessage>
            where TMessage : class, IServiceMessage
        {
            this._serviceCollection.AddTransient<IMessageHandler<TMessage>, THandler>();
            this._serviceCollection.AddTransient<THandler>();

            this.Subscribe<THandler, TMessage>();

            return this;
        }

        internal void Build()
        {
            this._serviceCollection.AddSingleton(this._subscriptions);
        }
    }
}