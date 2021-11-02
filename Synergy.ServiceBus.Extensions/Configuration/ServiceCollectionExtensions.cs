using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;
using Synergy.ServiceBus.Extensions.Filters;
using Synergy.ServiceBus.Extensions.Progress;
using Synergy.ServiceBus.Extensions.Serialization;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public static class ServiceCollectionExtensions
    {
        private const string ObsoleteMsg = @"Use AddServiceBus and handlers with configuration delegate in combination with UseServiceBusAsync/UseServiceBus on IServiceBus instance.";

        public static IServiceCollection AddMessageFiltering(this IServiceCollection services)
        {
            return services.AddSingleton<IFilterProvider, DefaultFilterProvider>();
        }

        public static IServiceCollection AddMessageFilter<T, TMessage>(this IServiceCollection services)
            where T : class, IMessageFilter<TMessage>
            where TMessage : IMessage
        {
            return services.AddSingleton<IMessageFilter, T>();
        }

        public static IServiceCollection AddMessageFilter<T>(this IServiceCollection services)
            where T : class, IMessageFilter
        {
            return services.AddSingleton<IMessageFilter, T>();
        }

        public static IServiceCollection AddMessageCorrelation(this IServiceCollection services)
        {
            return services.AddMessageFilter<OperationContextFilter>();
        }

        public static IServiceCollection AddBusLogEnricher(this IServiceCollection services)
        {
            return services.AddMessageFilter<LogEnricherFilter>();
        }

        public static IServiceCollection AddStatusMessages(this IServiceCollection services)
        {
            services.AddSingleton<ProgressScopeStorage>();
            services.AddSingleton<IProgressScopeFactory>(x => x.GetRequiredService<ProgressScopeStorage>());
            services.AddSingleton<IProgressScopeAccessor>(x => x.GetRequiredService<ProgressScopeStorage>());
            services.AddMessageFilter<EventProgressFilter>();
            services.AddMessageFilter<CommandProgressFilter>();
            services.AddTransient<IProgressPublisher, ProgressPublisher>();
            return services;
        }

        public static IServiceCollection AddDefaultFilters(this IServiceCollection services)
        {
            return services
                .AddMessageFiltering()
                .AddStatusMessages()
                //.AddMessageCorrelation()
                .AddBusLogEnricher();
        }

        [Obsolete(ObsoleteMsg)]
        public static IServiceCollection AddMessageHandler<T, TMessage>(this IServiceCollection services)
            where T : class, IMessageHandler<TMessage>
            where TMessage : IMessage
        {
            services.AddTransient<IMessageHandler<TMessage>, T>();
            services.AddTransient<T>();

            return services;
        }

        public static IServiceCollection AddMessageSerializer<T>(this IServiceCollection services)
            where T : class, IMessageSerializer
        {
            services.AddTransient<IMessageSerializer, T>();
            services.AddTransient<T>();

            return services;
        }

        public static IServiceCollection AddJsonMessageSerializer(this IServiceCollection services)
        {
            services.RemoveAll<IMessageSerializer>();

            services.AddTransient<IMessageSerializer, JsonMessageSerializer>();

            return services;
        }

        public static IServiceCollection AddLargeMessageSerializer(this IServiceCollection services)
        {
            services.RemoveAll<IMessageSerializer>();

            services.AddTransient<JsonMessageSerializer>();
            services.AddTransient<IMessageSerializer, ExternalStorageSerializer>();

            return services;
        }

        public static IServiceCollection AddServiceBus<TBus, TConfig>(this IServiceCollection services, Action<HandlerRegistrationBuilder<TConfig>> configure)
            where TConfig : class
            where TBus : class, IMessageBus
        {
            var builder = new HandlerRegistrationBuilder<TConfig>(services);

            configure(builder);

            builder.Build();

            services.AddDefaultFilters();

            services.AddSingleton<IMessageBus, TBus>();
            services.AddSingleton<IPublishMessage>(provider => provider.GetService<IMessageBus>());

            return services;
        }
    }
}