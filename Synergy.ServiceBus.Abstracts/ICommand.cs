using System;

namespace Synergy.ServiceBus.Abstracts
{
    public interface ICommand : IMessage<Guid>
    {
    }
}
