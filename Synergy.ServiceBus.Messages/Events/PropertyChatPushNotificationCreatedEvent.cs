using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public class PropertyChatPushNotificationCreatedEvent : Event
    {
        public Guid PropertyId { get; set; }

        public Guid UserId { get; set; }

        public string ChatMessage { get; set; }

        public string TaggedUserList { get; set; }
    }
}
