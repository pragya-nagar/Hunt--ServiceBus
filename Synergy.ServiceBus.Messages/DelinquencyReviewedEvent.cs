using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class DelinquencyReviewedEvent : Event
    {
        public Guid EventId { get; set; }

        public int Order { get; set; }

        public bool LevelReviewFinished { get; set; }

        public Guid? NextReviewerId { get; set; }
    }
}
