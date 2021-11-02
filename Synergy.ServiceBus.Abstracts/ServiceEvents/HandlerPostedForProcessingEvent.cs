using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class HandlerPostedForProcessingEvent : IServiceMessage
    {
        public HandlerPostedForProcessingEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}