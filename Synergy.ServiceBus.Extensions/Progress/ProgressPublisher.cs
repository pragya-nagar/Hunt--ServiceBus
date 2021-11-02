using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Messages;

namespace Synergy.ServiceBus.Extensions.Progress
{
    public class ProgressPublisher : IProgressPublisher
    {
        private readonly ILogger<ProgressPublisher> _logger;

        private readonly IPublishMessage _publisher;

        private readonly IProgressScopeAccessor _progressScopeAccessor;
        private readonly IProgressScopeFactory _progressScopeFactory;

        public ProgressPublisher(
            ILogger<ProgressPublisher> logger,
            IPublishMessage publishMessage,
            IProgressScopeAccessor progressScopeAccessor,
            IProgressScopeFactory progressScopeFactory)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._publisher = publishMessage ?? throw new ArgumentNullException(nameof(publishMessage));
            this._progressScopeAccessor = progressScopeAccessor ?? throw new ArgumentNullException(nameof(progressScopeAccessor));
            this._progressScopeFactory = progressScopeFactory ?? throw new ArgumentNullException(nameof(progressScopeFactory));
        }

        public virtual async Task PostProgressAsync(int progress, CancellationToken cancellationToken)
        {
            if (this._progressScopeAccessor.Current == null)
            {
                this._logger.LogWarning("Progress scope was not found. Operation progress was not published.");
                return;
            }

            this._progressScopeAccessor.Current.CurrentProgress = progress;

            var evt = Event.Create<OperationStatusEvent>(Guid.NewGuid(), this._progressScopeAccessor.Current.UserId);
            evt.Code = HttpStatusCode.Accepted;
            evt.Message = "InProgress";
            evt.Progress = (int)this._progressScopeAccessor.Current.TotalProgress;

            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public virtual async Task IncrementProgressAsync(int increment, CancellationToken cancellationToken)
        {
            if (this._progressScopeAccessor.Current == null)
            {
                this._logger.LogWarning("Progress scope was not found. Operation progress was not published.");
                return;
            }

            this._progressScopeAccessor.Current.CurrentProgress += increment;

            var evt = Event.Create<OperationStatusEvent>(Guid.NewGuid(), this._progressScopeAccessor.Current.UserId);

            evt.Code = HttpStatusCode.Accepted;
            evt.Message = "InProgress";
            evt.Progress = (int)this._progressScopeAccessor.Current.TotalProgress;

            await this._publisher.PublishAsync(evt, cancellationToken).ConfigureAwait(false);
        }

        public IDisposable CreateChildScope(int scopeSize)
        {
            if (this._progressScopeAccessor.Current == null)
            {
                throw new ApplicationException("Enable to create child progress scope. Current scope is not defined.");
            }

            var childScope = new ProgressScope(this._progressScopeAccessor.Current, scopeSize);

            this._progressScopeFactory.Create(childScope);

            return new ScopeDestructor(this._progressScopeFactory, childScope);
        }

        private class ScopeDestructor : IDisposable
        {
            private readonly IProgressScopeFactory _progressScopeFactory;
            private readonly ProgressScope _current;

            public ScopeDestructor(IProgressScopeFactory progressScopeFactory, ProgressScope current)
            {
                this._progressScopeFactory = progressScopeFactory;
                this._current = current;
            }

            public void Dispose()
            {
                if (this._current.Parent != null)
                {
                    this._current.Parent.CurrentProgress += this._current.ScopeSize * (this._current.CurrentProgress / 100);
                }

                this._progressScopeFactory.Create(this._current.Parent);
            }
        }
    }
}