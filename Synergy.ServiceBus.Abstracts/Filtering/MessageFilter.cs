using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public abstract class MessageFilter<T> : IMessageFilter<T>
        where T : class, IMessage
    {
        Task<FilterExecutionResult> IMessageFilter.ReceiveMessageAsync(IMessage message, FilterExecutionContext context)
            => (this as IMessageFilter<T>).ReceiveMessageAsync((T)message, context);

        Task<FilterExecutionResult> IMessageFilter.PreSendMessageAsync(IMessage message, FilterExecutionContext context)
            => (this as IMessageFilter<T>).PreSendMessageAsync((T)message, context);

        Task IMessageFilter.PreHandleMessageAsync(IMessage message, HandleOptions options, FilterExecutionContext context)
            => (this as IMessageFilter<T>).PreHandleMessageAsync((T)message, options, context);

        Task IMessageFilter.PostHandleMessageAsync(IMessage message, FilterExecutionContext context)
            => (this as IMessageFilter<T>).PostHandleMessageAsync((T)message, context);

        public virtual Task<FilterExecutionResult> ReceiveMessageAsync(T message, FilterExecutionContext context) => Task.FromResult(FilterExecutionResult.Sucess());

        public virtual Task<FilterExecutionResult> PreSendMessageAsync(T message, FilterExecutionContext context) => Task.FromResult(FilterExecutionResult.Sucess());

        public virtual Task PostHandleMessageAsync(T message, FilterExecutionContext context) => Task.CompletedTask;

        public virtual Task PreHandleMessageAsync(T message, HandleOptions options, FilterExecutionContext context) => Task.CompletedTask;
    }
}