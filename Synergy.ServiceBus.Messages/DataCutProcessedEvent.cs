using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class DataCutProcessedEvent : Event
    {
        public int ManualDelinquencyCount { get; set; }

        public IEnumerable<(int Index, int Level, IEnumerable<(Guid UserId, DateTime? DecisionDate)> Users)> NLevelUsers { get; set; }

        public IEnumerable<(Guid UserId, DateTime? DecisionDate)> FinalLevelUsers { get; set; }
    }
}