using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class ETLProcessingFinishedEvent : Event
    {
        public DateTime EndTime { get; set; }
    }
}
