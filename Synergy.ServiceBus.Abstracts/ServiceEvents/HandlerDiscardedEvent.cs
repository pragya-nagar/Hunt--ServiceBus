using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class HandlerDiscardedEvent : IServiceMessage
    {
        public HandlerDiscardedEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}