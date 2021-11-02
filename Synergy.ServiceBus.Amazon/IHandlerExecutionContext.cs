using System;
using System.Threading;
using System.Threading.Tasks;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Amazon
{
    internal interface IHandlerExecutionContext : IDisposable
    {
        MessageContext MessageContext { get; }

        HandleOptions Options { get; }

        DateTime? HandlerStartedTimestamp { get; set; }

        IMessageHandler Handler { get; }

        Task ExecuteAsync(dynamic message, CancellationToken cancellationToken);
    }
}