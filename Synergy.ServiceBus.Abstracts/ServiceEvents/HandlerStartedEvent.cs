using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class HandlerStartedEvent : IServiceMessage
    {
        public HandlerStartedEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}