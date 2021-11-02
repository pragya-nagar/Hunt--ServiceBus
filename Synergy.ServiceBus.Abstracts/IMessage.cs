using System;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IMessage<out T> : IMessage
    {
        T Id { get; }
    }

    public interface IMessage
    {
        DateTime CreatedOn { get; }
    }

    public interface IServiceMessage : IMessage
    {
    }
}
