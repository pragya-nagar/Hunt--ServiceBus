using System;

namespace Synergy.ServiceBus.Abstracts
{
    public class HandlerData
    {
        public HandleOptions Options { get; set;  }

        public DateTime? HandlerStartedTimestamp { get; set; }

        public IMessageHandler Handler { get; set; }
    }
}