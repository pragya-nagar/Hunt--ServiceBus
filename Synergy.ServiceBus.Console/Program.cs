using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Synergy.Common.Logging.Configuration;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.Console.Messages;
using Synergy.ServiceBus.Extensions.Configuration;
using Synergy.ServiceBus.Extensions.Handlers;

namespace Synergy.ServiceBus.Console
{
    internal class Program
    {
        private static async Task Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection()
                .AddLogging()
                .AddMessageFiltering()
                //.AddMessageCorrelation()
                .AddBusLogEnricher()
                .AddSerilogLogging(configuration)

                .RegisterServices(configuration, true);

            var serviceProvider = services.BuildServiceProvider();

            var bus = serviceProvider.GetService<IMessageBus>();

            bus.UseServiceBus(serviceProvider);

            while (true)
            {
                var cFactory = serviceProvider.GetRequiredService<ICorrelationContextFactory>();
                cFactory.Create(Guid.NewGuid().ToString(), "Custom");

                //var e1 = Event.Create<ETLProcessingFinishedEvent>(Guid.NewGuid(), Guid.NewGuid());
                //await bus.PublishAsync(e1);

                var cmd1 = Command.Create<TestCommand1>(Guid.NewGuid(), Guid.NewGuid());
                await bus.PublishAsync(cmd1);

                //var cmd2 = Command.Create<TestCommand2>(Guid.NewGuid(), Guid.NewGuid());
                //await bus.PublishAsync(cmd2);

                await Task.Delay(TimeSpan.FromMinutes(10));
            }
        }
    }

    public class NotificationHandler : ServiceHandler
    {
        public override Task HandleAsync(HandlerDiscardedEvent message, CancellationToken cancellationToken)
        {
            return base.HandleAsync(message, cancellationToken);
        }

        public override Task HandleAsync(DeadMessageEvent message, CancellationToken cancellationToken)
        {
            return base.HandleAsync(message, cancellationToken);
        }

        public override Task HandleAsync(HandlerStartedEvent message, CancellationToken cancellationToken)
        {
            return base.HandleAsync(message, cancellationToken);
        }

        public override Task HandleAsync(HandlerSuccessEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}
