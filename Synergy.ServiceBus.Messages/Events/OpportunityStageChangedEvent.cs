using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class OpportunityStageChangedEvent : Event
    {
        public string OpportunityNumber { get; set; }

        public int? PreviousStageId { get; set; }

        public int CurrentStageId { get; set; }
    }
}