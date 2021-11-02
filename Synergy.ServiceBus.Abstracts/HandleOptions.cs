using System;

namespace Synergy.ServiceBus.Abstracts
{
    public class HandleOptions
    {
        public TimeSpan ExecutionTimeout { get; set; } = TimeSpan.FromMinutes(2);

        public bool DisableParallelProcessing { get; set; }

        public bool IsTerminal { get; set; } = true;
    }
}