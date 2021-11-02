using Synergy.ServiceBus.Extensions.Progress;
using Synergy.ServiceBus.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Console.Messages;
using Synergy.ServiceBus.Messages.Events;

namespace Synergy.ServiceBus.Console.Handlers
{

    public class OpStatus : IMessageHandler<OperationStatusEvent>
    {
        public void Handle(OperationStatusEvent message)
        {
            HandleAsync(message, default).GetAwaiter().GetResult();
        }

        public async Task HandleAsync(OperationStatusEvent message, CancellationToken cancellationToken)
        {
            await Task.Yield();
        }
    }

    public class TestCommandHandler :
        IMessageHandler<ETLProcessingFinishedEvent>,
        IMessageHandler<TestCommand1>,
        IMessageHandler<TestCommand2>,
        IMessageHandler<TestCommand3>,
        IMessageHandler<TestCommand4>,
        IMessageHandler<TestCommand5>,
        IMessageHandler<TestCommand6>,
        IMessageHandler<TestCommand7>,
        IMessageHandler<TestCommand8>,
        IMessageHandler<TestCommand9>,
        IMessageHandler<TestCommand10>,
        IMessageHandler<TestCommand11>,
        IMessageHandler<TestCommand12>,
        IMessageHandler<TestCommand13>,
        IMessageHandler<TestCommand14>,
        IMessageHandler<TestCommand15>,
        IMessageHandler<TestCommand16>,
        IMessageHandler<TestCommand17>,
        IMessageHandler<TestCommand18>,
        IMessageHandler<TestCommand19>,
        IMessageHandler<TestCommand20>,
        IMessageHandler<TestCommand21>,
        IMessageHandler<TestCommand22>,
        IMessageHandler<TestCommand23>,
        IMessageHandler<TestCommand24>,
        IMessageHandler<TestCommand25>,
        IMessageHandler<TestCommand26>,
        IMessageHandler<TestCommand27>,
        IMessageHandler<TestCommand28>,
        IMessageHandler<TestCommand29>,
        IMessageHandler<TestCommand30>
    {
        private readonly ILogger<TestCommandHandler> _logger;
        private readonly IMessageBus _mesBus;
        private readonly IProgressPublisher _progressPublisher;

        public TestCommandHandler(ILogger<TestCommandHandler> logger, IMessageBus mesBus, IProgressPublisher progressPublisher)
        {
            this._logger = logger;
            this._mesBus = mesBus;
            this._progressPublisher = progressPublisher;
        }

        public void Handle(TestCommand1 message)
        {
            this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand1 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogError($"I have received a message {message.Id} [{message.GetType()}]");

            await this._progressPublisher.PostProgressAsync(50, default).ConfigureAwait(false); // 50

            using (this._progressPublisher.CreateChildScope(30))
            {
                await this._progressPublisher.PostProgressAsync(20, default).ConfigureAwait(false); // 56

                using (this._progressPublisher.CreateChildScope(80))
                {
                    using (this._progressPublisher.CreateChildScope(50))
                    {
                        await this._progressPublisher.IncrementProgressAsync(50, default).ConfigureAwait(false); // 62
                        await this._progressPublisher.IncrementProgressAsync(50, default).ConfigureAwait(false); // 68
                    }

                    await this._progressPublisher.IncrementProgressAsync(50, default).ConfigureAwait(false);    // 80
                }
            }

            await this._progressPublisher.IncrementProgressAsync(10, default).ConfigureAwait(false); // 90

            //for (int i = 0; i < 6 * 60; i++)
            //{
            //    if (cancellationToken.IsCancellationRequested)
            //    {
            //        this._logger.LogWarning("!!! Handler canceled !!!");
            //        return;
            //    }

            //    this._logger.LogDebug("Message processing step {step}", i + 1);

            //    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            //}

            await this._mesBus.PublishAsync(Command.Create<TestCommand2>(Guid.NewGuid(), Guid.NewGuid()), cancellationToken);
        }

        public void Handle(TestCommand2 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand2 message, CancellationToken cancellationToken = default)
        {
            //this._logger.LogError($"I have received a message {message.Id} [{message.GetType()}]");
            //await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);
            await Task.CompletedTask;
        }

        public void Handle(TestCommand3 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand3 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand4 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand4 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand5 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand5 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand6 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand6 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand7 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand7 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand8 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand8 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand9 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand9 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand10 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand10 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand11 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand11 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand12 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand12 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand13 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand13 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand14 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand14 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand15 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand15 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand16 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand16 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand17 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand17 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand18 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand18 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand19 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand19 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand20 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand20 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand21 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand21 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand22 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand22 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand23 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand23 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand24 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand24 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand25 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand25 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand26 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand26 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand27 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand27 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand28 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand28 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand29 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand29 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(TestCommand30 message)
        {
             this.HandleAsync(message).Wait();
        }

        public async Task HandleAsync(TestCommand30 message, CancellationToken cancellationToken = default)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
            await Task.CompletedTask;
        }

        public void Handle(ETLProcessingFinishedEvent message)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");
        }

        public Task HandleAsync(ETLProcessingFinishedEvent message, CancellationToken cancellationToken)
        {
            this._logger.LogInformation($"I have received a message {message.Id} [{message.GetType()}]");

            return Task.CompletedTask;
        }
    }
}
