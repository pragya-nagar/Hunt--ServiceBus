using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public interface IMessageFilter
    {
        Task<FilterExecutionResult> ReceiveMessageAsync(IMessage message, FilterExecutionContext context);

        Task<FilterExecutionResult> PreSendMessageAsync(IMessage message, FilterExecutionContext context);

        Task PreHandleMessageAsync(IMessage message, HandleOptions options, FilterExecutionContext context);

        Task PostHandleMessageAsync(IMessage message, FilterExecutionContext context);
    }

    public interface IMessageFilter<in T> : IMessageFilter
        where T : IMessage
    {
        Task<FilterExecutionResult> ReceiveMessageAsync(T message, FilterExecutionContext context);

        Task<FilterExecutionResult> PreSendMessageAsync(T message, FilterExecutionContext context);

        Task PreHandleMessageAsync(T message, HandleOptions options, FilterExecutionContext context);

        Task PostHandleMessageAsync(T message, FilterExecutionContext context);
    }
}