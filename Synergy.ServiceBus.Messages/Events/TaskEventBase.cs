using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages.Events
{
    public abstract class TaskEventBase<T> : Event
        where T : class, new()
    {
        public Guid TaskId { get; set; }

        public Guid UserId { get; set; }

        public int NotificationTypes { get; set; }

        public T TaskMetadata { get; set; } = new T();
    }
}