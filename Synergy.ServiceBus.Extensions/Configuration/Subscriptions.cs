using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Extensions.Configuration
{
    public class Subscriptions
    {
        internal List<(Func<IMessageBus, IServiceProvider, HandleOptions, CancellationToken, Task> registerFunc, HandleOptions options)> Registrations { get; } = new List<(Func<IMessageBus, IServiceProvider, HandleOptions, CancellationToken, Task> registerFunc, HandleOptions options)>();
    }
}