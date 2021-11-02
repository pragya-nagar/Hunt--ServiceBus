using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class UserRegisteredEvent : Event
    {
        public string UserName { get; set; }

        public string Email { get; set; }
    }
}
