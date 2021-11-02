using System;
using System.Collections.Generic;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class LoanStateChangedEvent : Event
    {
        public IEnumerable<int> CompletedStages { get; set; }

        public int CurrentStage { get; set; }

        public IEnumerable<Guid> Assignees { get; set; }

        public string LoanNumber { get; set; }
    }
}