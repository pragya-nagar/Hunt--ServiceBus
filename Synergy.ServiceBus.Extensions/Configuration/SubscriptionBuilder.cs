using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public class SubscriptionBuilder
    {
        private readonly List<Func<IMessageBus, IServiceProvider, CancellationToken, Task>> _subscriptions = new List<Func<IMessageBus, IServiceProvider, CancellationToken, Task>>();

        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _serviceProvider;
        private readonly CancellationToken _cancellationToken;

        public SubscriptionBuilder(IMessageBus messageBus, IServiceProvider serviceProvider, CancellationToken cancellationToken)
        {
            this._messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
            this._serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            this._cancellationToken = cancellationToken;
        }

        public SubscriptionBuilder Subscribe<TMessage>(HandleOptions handleOptions = null)
            where TMessage : class, IMessage
        {
            this._subscriptions.Add((messageBus, provider, token) => messageBus.SubscribeAsync<TMessage>(provider, handleOptions, token));

            return this;
        }

        public SubscriptionBuilder Subscribe<TMessage, THandler>(HandleOptions handleOptions = null)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage
        {
            this._subscriptions.Add((messageBus, provider, token) => messageBus.SubscribeAsync<TMessage, THandler>(provider, handleOptions, token));

            return this;
        }

        public SubscriptionBuilder SubscribeWithOptions<TMessage, THandler>(TimeSpan timeout, bool disableParallel = false)
            where THandler : class, IMessageHandler<TMessage>
            where TMessage : class, IMessage
        {
            this.Subscribe<TMessage, THandler>(new HandleOptions
            {
                ExecutionTimeout = timeout,
                DisableParallelProcessing = disableParallel,
            });

            return this;
        }

        public SubscriptionBuilder SubscribeToServiceEvents<TMessage, THandler>()
            where THandler : class, IServiceHandler<TMessage>
            where TMessage : class, IServiceMessage
        {
            this.Subscribe<TMessage, THandler>();

            return this;
        }

        public async Task ApplyAsync()
        {
            this._cancellationToken.ThrowIfCancellationRequested();

            foreach (var subscription in this._subscriptions)
            {
                await subscription(_messageBus, _serviceProvider, _cancellationToken).ConfigureAwait(false);
            }
        }
    }
}