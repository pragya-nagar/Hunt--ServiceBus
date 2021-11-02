using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class UserDeactivatedEvent : Event
    {
        public Guid UserId { get; set; }

        public string UserFullName { get; set; }

        public IEnumerable<Guid> ManagerIds { get; set; }

        public DateTime EventDateTime { get; set; }
    }
}
