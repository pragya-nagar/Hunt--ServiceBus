using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using Amazon.SQS.Model;

namespace Synergy.ServiceBus.Amazon
{
    [DebuggerDisplay("{" + nameof(MessageType) + "}")]
    internal class MessageContext : IDisposable
    {
        public MessageContext(
            DateTime receiveTimestamp,
            dynamic message,
            Type messageType,
            Message queueMessage,
            Dictionary<string, string> metadata)
        {
            this.QueueMessage = queueMessage;
            this.Metadata = metadata;

            this.ReceiveTimestamp = receiveTimestamp;
            this.Message = message;
            this.MessageType = messageType;

            this.ReceivedCount = int.Parse(queueMessage.Attributes["ApproximateReceiveCount"], CultureInfo.InvariantCulture);
        }

        public DateTime ReceiveTimestamp { get; }

        public int ReceivedCount { get; }

        public dynamic Message { get; }

        public Type MessageType { get; }

        public Message QueueMessage { get; }

        public Dictionary<string, string> Metadata { get; }

        public ConcurrentDictionary<IHandlerExecutionContext, byte> Handlers { get; private set; } = new ConcurrentDictionary<IHandlerExecutionContext, byte>();

        public TimeSpan CurrentVisibility { get; set; }

        public DateTime? LastRenewTimestamp { get; set; }

        public IHandlerExecutionContext AttachHandler(IHandlerExecutionContext handlerContext)
        {
            this.Handlers.TryAdd(handlerContext, 0);

            return handlerContext;
        }

        public void DetachHandler(IHandlerExecutionContext handlerContext)
        {
            this.Handlers.TryRemove(handlerContext, out _);
        }

        public void Dispose()
        {
            foreach (var handler in this.Handlers.Keys)
            {
                handler.Dispose();
            }

            this.Handlers.Clear();
        }
    }
}