using System;
using System.Reflection;
using Amazon;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Synergy.Common.Abstracts;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.Amazon;
using Synergy.ServiceBus.Console.Handlers;
using Synergy.ServiceBus.Console.Messages;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.ServiceBus.Messages;
using Synergy.ServiceBus.Messages.Events;
using Synergy.ServiceBus.RabbitMq;

namespace Synergy.ServiceBus.Console
{
    public static class ServicesRegistration
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, IConfiguration config, bool isDevelopment)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config));
            }

            if (isDevelopment)
            {
                services.AddServiceBus<Synergy.ServiceBus.RabbitMq.MessageBus, RabbitMQConfig>(builder =>
                {
                    builder.Configure(config, "ServiceBus:RabbitMQ");

                    AddSubscriptions(builder);
                });
            }
            else
            {
                services.AddServiceBus<Synergy.ServiceBus.Amazon.MessageBus, AWSMessageBusConfig>(builder =>
                {
                    builder.Configure(x =>
                    {
                        config.Bind("AwsMessageBus", x);

                        x.TopicName = config["TopicName"];
                        x.QueueName = config["crm:QueueName"];
                       // x.Region = config.GetRegionEndPoint();
                    });

                    AddSubscriptions(builder);
                });

                services.AddLargeMessageSerializer();
            }

            void AddSubscriptions(IHandlerRegistrationBuilder builder)
            {
                //builder.Subscribe<TestCommandHandler, TestCommand1>(new HandleOptions() { IsTerminal = false });
                //builder.Subscribe<TestCommandHandler, TestCommand2>();
                //builder.Subscribe<TestCommandHandler, TestCommand3>();
                //builder.Subscribe<TestCommandHandler, TestCommand4>();
                //builder.Subscribe<TestCommandHandler, TestCommand5>();
                //builder.Subscribe<TestCommandHandler, TestCommand6>();
                //builder.Subscribe<TestCommandHandler, TestCommand7>();
                //builder.Subscribe<TestCommandHandler, TestCommand8>();
                //builder.Subscribe<TestCommandHandler, TestCommand9>();
                //builder.Subscribe<TestCommandHandler, TestCommand10>();
                //builder.Subscribe<TestCommandHandler, TestCommand11>();
                //builder.Subscribe<TestCommandHandler, TestCommand12>();
                //builder.Subscribe<TestCommandHandler, TestCommand13>();
                //builder.Subscribe<TestCommandHandler, TestCommand14>();
                //builder.Subscribe<TestCommandHandler, TestCommand15>();
                //builder.Subscribe<TestCommandHandler, TestCommand16>();
                //builder.Subscribe<TestCommandHandler, TestCommand17>();
                //builder.Subscribe<TestCommandHandler, TestCommand18>();
                //builder.Subscribe<TestCommandHandler, TestCommand19>();
                //builder.Subscribe<TestCommandHandler, TestCommand20>();

                //builder.Subscribe<TestCommandHandler, ETLProcessingFinishedEvent>();
                builder.Subscribe<OpStatus, OperationStatusEvent>();

                builder.SubscribeToServiceEvent<NotificationHandler, HandlerDiscardedEvent>();
                builder.SubscribeToServiceEvent<NotificationHandler, HandlerStartedEvent>();
                builder.SubscribeToServiceEvent<NotificationHandler, DeadMessageEvent>();
                builder.SubscribeToServiceEvent<NotificationHandler, HandlerSuccessEvent>();
            };

            services.AddStatusMessages();

            services.AddSingleton<IHostingEnvironment>(new HostingEnvironment
            {
                ApplicationName = Assembly.GetEntryAssembly().GetName().Name,
                EnvironmentName = "Development"
            });

            services.AddHostedService<StartupService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            return services;
        }
    }
}
