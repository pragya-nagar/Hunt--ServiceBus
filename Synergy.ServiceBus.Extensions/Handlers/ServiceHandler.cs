using System.Threading;
using System.Threading.Tasks;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.ServiceEvents;

namespace Synergy.ServiceBus.Extensions.Handlers
{
    public abstract class ServiceHandler :
        IServiceHandler<MessageReceivedEvent>,
        IServiceHandler<DeadMessageEvent>,
        IServiceHandler<HandlerStartedEvent>,
        IServiceHandler<HandlerDiscardedEvent>,
        IServiceHandler<HandlerPostedForProcessingEvent>,
        IServiceHandler<HandlerSuccessEvent>,
        IServiceHandler<HandlerExceptionEvent>
    {
        public void Handle(MessageReceivedEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(MessageReceivedEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(DeadMessageEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(DeadMessageEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(HandlerStartedEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(HandlerStartedEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(HandlerDiscardedEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(HandlerDiscardedEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(HandlerPostedForProcessingEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(HandlerPostedForProcessingEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(HandlerSuccessEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(HandlerSuccessEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public void Handle(HandlerExceptionEvent message)
        {
            this.HandleAsync(message, CancellationToken.None).GetAwaiter().GetResult();
        }

        public virtual Task HandleAsync(HandlerExceptionEvent message, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
}