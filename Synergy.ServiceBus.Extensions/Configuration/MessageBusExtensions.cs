using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public static class MessageBusExtensions
    {
        public static async Task<IMessageBus> UseServiceBusAsync(this IMessageBus bus, IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
        {
            var subscriptions = serviceProvider.GetService<Subscriptions>();

            foreach (var registration in subscriptions.Registrations)
            {
                await registration.registerFunc(bus, serviceProvider, registration.options, cancellationToken).ConfigureAwait(false);
            }

            await bus.StartListeningAsync(cancellationToken).ConfigureAwait(false);

            return bus;
        }

        public static IMessageBus UseServiceBus(this IMessageBus bus, IServiceProvider serviceProvider)
        {
            return UseServiceBusAsync(bus, serviceProvider, default).GetAwaiter().GetResult();
        }

        public static IApplicationBuilder UseServiceBus(this IApplicationBuilder builder)
        {
            var serviceProvider = builder.ApplicationServices;

            serviceProvider.GetRequiredService<IMessageBus>().UseServiceBus(serviceProvider);

            return builder;
        }

        internal static async Task<IMessageBus> SubscribeAsync<TMessage>(
            this IMessageBus messageBus,
            IServiceProvider serviceProvider,
            HandleOptions options,
            CancellationToken cancellationToken = default)
            where TMessage : class, IMessage
        {
            await messageBus.SubscribeAsync(() => new HandlerScope<TMessage, IMessageHandler<TMessage>>(serviceProvider, options), cancellationToken).ConfigureAwait(false);

            return messageBus;
        }

        internal static async Task<IMessageBus> SubscribeAsync<TMessage, TMessageHandler>(
            this IMessageBus messageBus,
            IServiceProvider serviceProvider,
            HandleOptions options,
            CancellationToken cancellationToken = default)
            where TMessage : class, IMessage
            where TMessageHandler : class, IMessageHandler<TMessage>
        {
            await messageBus.SubscribeAsync(() => new HandlerScope<TMessage, TMessageHandler>(serviceProvider, options), cancellationToken).ConfigureAwait(false);

            return messageBus;
        }
    }
}