using System;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Console
{
    public class StartupService : IHostedService
    {
        private readonly ILogger _logger;
        private readonly IMessageBus _messageBus;
        private readonly IServiceProvider _provider;

        public StartupService(ILogger<StartupService> logger, IMessageBus messageBus, IServiceProvider provider)
        {
            this._logger = logger;
            this._messageBus = messageBus;
            this._provider = provider;
        }

        public async Task StartAsync(CancellationToken cancellationToken = default)
        {
            await this.SubscribeHandlersAsync(cancellationToken).ConfigureAwait(false);
            await _messageBus.StartListeningAsync(cancellationToken).ConfigureAwait(false);
        }

        public Task StopAsync(CancellationToken cancellationToken = default)
        {
            ((IDisposable)this._messageBus).Dispose();

            return Task.CompletedTask;
        }

        private Task SubscribeHandlersAsync(CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }
}
