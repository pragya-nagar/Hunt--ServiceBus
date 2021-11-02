using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;
using Synergy.ServiceBus.Extensions.Progress;

namespace Synergy.ServiceBus.Extensions.Filters
{
    public class EventProgressFilter : MessageFilter<Event>
    {
        private const string MetadataKey = "Progress";

        private readonly ILoggerFactory _loggerFactory;

        private readonly IProgressScopeFactory _progressScopeFactory;

        private readonly IProgressScopeAccessor _progressScopeAccessor;

        public EventProgressFilter(ILoggerFactory loggerFactory, IProgressScopeFactory progressScopeFactory, IProgressScopeAccessor progressScopeAccessor)
        {
            this._loggerFactory = loggerFactory;
            this._progressScopeFactory = progressScopeFactory;
            this._progressScopeAccessor = progressScopeAccessor;
        }

        public override Task<FilterExecutionResult> PreSendMessageAsync(Event message, FilterExecutionContext filterExecutionContext)
        {
            var progress = this._progressScopeAccessor.Current?.CurrentProgress;
            if (progress.HasValue && progress.Value > 0)
            {
                filterExecutionContext.Metadata[MetadataKey] = ((int)progress.Value).ToString(CultureInfo.InvariantCulture);
            }

            return Task.FromResult(FilterExecutionResult.Sucess());
        }

        public override Task PreHandleMessageAsync(Event message, HandleOptions options, FilterExecutionContext context)
        {
            if (this._progressScopeAccessor.Current == null)
            {
                var progressScope = new ProgressScope(message.CreatedBy, options?.IsTerminal ?? true);

                if (context.Metadata.ContainsKey(MetadataKey) && int.TryParse(context.Metadata[MetadataKey], out var progress))
                {
                    progressScope.CurrentProgress = progress;
                }

                this._progressScopeFactory.Create(progressScope);
            }

            return Task.CompletedTask;
        }
    }
}