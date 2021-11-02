using System;
using System.Collections.Generic;

namespace Synergy.ServiceBus.Abstracts
{
    public class ServiceEvent
    {
        public IMessage Message { get; set; }

        public Dictionary<string, string> Metadata { get; set; }

        public DateTime ReceiveTimestamp { get; set;  }

        public int ReceivedCount { get; set; }

        public HandlerData HandlerData { get; set; }
    }
}