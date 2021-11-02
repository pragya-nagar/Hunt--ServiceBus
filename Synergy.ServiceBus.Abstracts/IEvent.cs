using System;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IEvent : IMessage<Guid>
    {
    }
}
