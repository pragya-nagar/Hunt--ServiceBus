using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class EventCreatedEvent : Event
    {
        public string Number { get; set; }

        public int StateId { get; set; }

        public int TypeId { get; set; }

        public DateTime SaleDate { get; set; }

        public DateTime? FundingDate { get; set; }

        public IDictionary<int, IEnumerable<Guid>> DepartmentUserIds { get; set; }
    }
}
