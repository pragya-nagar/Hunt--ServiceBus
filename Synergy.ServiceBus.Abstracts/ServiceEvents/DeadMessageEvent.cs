using System;

namespace Synergy.ServiceBus.Abstracts.ServiceEvents
{
    public class DeadMessageEvent : IServiceMessage
    {
        public DeadMessageEvent()
        {
            this.CreatedOn = DateTime.UtcNow;
        }

        public ServiceEvent EventArgs { get; set; }

        public DateTime CreatedOn { get; set; }
    }
}