using System;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.Extensions.Hosting;
using Serilog.Context;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;

namespace Synergy.ServiceBus.Extensions.Filters
{
    public class LogEnricherFilter : MessageFilter<IMessage>
    {
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IHostingEnvironment _hostingEnvironment;

        public LogEnricherFilter(ICorrelationContextAccessor correlationContextAccessor, IHostingEnvironment hostingEnvironment)
        {
            this._correlationContextAccessor = correlationContextAccessor ?? throw new ArgumentNullException(nameof(correlationContextAccessor));
            this._hostingEnvironment = hostingEnvironment ?? throw new ArgumentNullException(nameof(hostingEnvironment));
        }

        public override Task<FilterExecutionResult> ReceiveMessageAsync(IMessage message, FilterExecutionContext context)
        {
            this.EnrichLogContext(message);

            return Task.FromResult(FilterExecutionResult.Sucess());
        }

        public override Task PostHandleMessageAsync(IMessage message, FilterExecutionContext context)
        {
            this.EnrichLogContext(message);

            return Task.CompletedTask;
        }

        private void EnrichLogContext(IMessage message)
        {
            LogContext.PushProperty("CorrelationIdGUID", this._correlationContextAccessor.CorrelationContext?.CorrelationId);

            LogContext.PushProperty("MessageType", message.GetType().Name);

            LogContext.PushProperty("MessageType", message.GetType().Name);

            LogContext.PushProperty("Environment", $"{this._hostingEnvironment.ApplicationName}-{this._hostingEnvironment.EnvironmentName}");
        }
    }
}