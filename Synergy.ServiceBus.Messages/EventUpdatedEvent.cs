namespace Synergy.ServiceBus.Messages
{
    public class EventUpdatedEvent : EventCreatedEvent
    {
        public int ManualDelinquencyCount { get; set; }
    }
}