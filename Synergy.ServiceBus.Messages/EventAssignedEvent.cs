using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class EventAssignedEvent : Event
    {
        public IEnumerable<(int Index, int Level, IEnumerable<(Guid UserId, DateTime? DecisionDate)> Users)> NLevelUsers { get; set; }

        public IEnumerable<(Guid UserId, DateTime? DecisionDate)> FinalLevelUsers { get; set; }
    }
}