using System;
using System.Threading.Tasks;
using CorrelationId;
using Microsoft.Extensions.Logging;
using Synergy.Common.Abstracts;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;

namespace Synergy.ServiceBus.Extensions.Filters
{
    public class OperationContextFilter : MessageFilter<IMessage>
    {
        private readonly ICorrelationContextFactory _correlationContextFactory;
        private readonly ICorrelationContextAccessor _correlationContextAccessor;
        private readonly IOperationContextFactory _operationContextFactory;
        private readonly IOperationContextAccessor _operationContextAccessor;
        private readonly ILoggerFactory _loggerFactory;

        public OperationContextFilter(ICorrelationContextFactory correlationContextFactory,
                                      ICorrelationContextAccessor correlationContextAccessor,
                                      IOperationContextFactory operationContextFactory,
                                      IOperationContextAccessor operationContextAccessor,
                                      ILoggerFactory loggerFactory)
        {
            this._correlationContextFactory = correlationContextFactory;
            this._correlationContextAccessor = correlationContextAccessor;
            this._operationContextFactory = operationContextFactory;
            this._operationContextAccessor = operationContextAccessor;
            this._loggerFactory = loggerFactory;
        }

        public override Task<FilterExecutionResult> PreSendMessageAsync(IMessage message, FilterExecutionContext filterExecutionContext)
        {
            var contextCorrelationId = this._correlationContextAccessor.CorrelationContext?.CorrelationId;
            var operationContext = this._operationContextAccessor.Current;
            if (operationContext != null)
            {
                filterExecutionContext.Metadata["UserId"] = this._operationContextAccessor.Current.UserId.ToString();
                filterExecutionContext.Metadata["UserName"] = this._operationContextAccessor.Current.UserName;
            }

            if (string.IsNullOrWhiteSpace(contextCorrelationId) == false)
            {
                filterExecutionContext.Metadata["CorrelationId"] = contextCorrelationId;

                var messageType = message.GetType();

                this._loggerFactory.CreateLogger(messageType).LogInformation("CorrelationId was updated before send for message {messageType} from correlation context", messageType);
            }

            return Task.FromResult(FilterExecutionResult.Sucess());
        }

        public override Task<FilterExecutionResult> ReceiveMessageAsync(IMessage message, FilterExecutionContext filterExecutionContext)
        {
            var messageType = message.GetType();

            var logger = this._loggerFactory.CreateLogger(messageType);

            string correlationString;

            if (filterExecutionContext.Metadata.ContainsKey("CorrelationId") == false)
            {
                correlationString = Guid.NewGuid().ToString();

                logger.LogWarning("Message {messageType} does not provide CorrelationId property. New CorrelationId {newCorrelationId} was generated.", messageType.Name, correlationString);
            }
            else
            {
                correlationString = filterExecutionContext.Metadata["CorrelationId"];
                logger.LogInformation("Correlation context was updated using CorrelationId value from message {messageType}", messageType);
            }

            this._correlationContextFactory.Create(correlationString, "Message");

            if (filterExecutionContext.Metadata.ContainsKey("UserId") &&
                filterExecutionContext.Metadata.ContainsKey("UserName") &&
                Guid.TryParse(filterExecutionContext.Metadata["UserId"], out var userId))
            {
                logger.LogDebug("Operation Context context was updated from message {messageType}", messageType);

                this._operationContextFactory.Create(new OperationContext()
                {
                    UserId = userId,
                    CorrelationId = Guid.Parse(correlationString),
                    UserName = filterExecutionContext.Metadata["UserName"],
                });
            }
            else
            {
                var systemUserId = new Guid("00000000-0000-0000-0000-000000000001");
                var systemUserName = "System";

                this._operationContextFactory.Create(new OperationContext()
                {
                    UserId = systemUserId,
                    CorrelationId = Guid.Parse(correlationString),
                    UserName = systemUserName,
                });

                logger.LogWarning("Message {messageType} does not provide operation context properties. System user information was set.", messageType.Name, correlationString);
            }

            return Task.FromResult(FilterExecutionResult.Sucess());
        }

        private class OperationContext : IOperationContext
        {
            public Guid CorrelationId { get; set; }

            public Guid UserId { get; set; }

            public string UserName { get; set; }
        }
    }
}