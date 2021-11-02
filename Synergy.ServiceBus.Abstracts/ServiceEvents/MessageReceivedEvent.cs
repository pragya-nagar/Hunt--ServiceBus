using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class MessageReceivedEvent : IServiceMessage
    {
        public MessageReceivedEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}