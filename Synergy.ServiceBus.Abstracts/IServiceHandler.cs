namespace Synergy.ServiceBus.Abstracts
{
    public interface IServiceHandler<in TServiceMessage> : IMessageHandler<TServiceMessage>
        where TServiceMessage : IServiceMessage
    {
    }
}