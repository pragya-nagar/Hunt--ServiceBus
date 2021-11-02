using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class HandlerExceptionEvent : IServiceMessage
    {
        public HandlerExceptionEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public bool Handled { get; set; }

        public ServiceEvent EventArgs { get; set; }

        public Exception Exception { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}