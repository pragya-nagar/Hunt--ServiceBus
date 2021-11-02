using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class HandlerSuccessEvent : IServiceMessage
    {
        public HandlerSuccessEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}
