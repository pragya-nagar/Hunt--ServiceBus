using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.Amazon.Extensions;
using MessageAttributeValue = Amazon.SimpleNotificationService.Model.MessageAttributeValue;

namespace Synergy.ServiceBus.Amazon
{
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly Semaphore _semaphore = new Semaphore(1, 1);

        private readonly ILogger _logger;

        private readonly IAmazonSQS _sqsClient;
        private readonly IAmazonSimpleNotificationService _snsClient;
        private readonly IMessageSerializer _serializer;

        private readonly AWSMessageBusConfig _config;

        private readonly IFilterProvider _filterProvider;

        private readonly TimeSpan _messageHandlingTimeout = TimeSpan.FromSeconds(30);

        private readonly int _maxParallellCapacity;

        private readonly int _maxThrottledCapacity;

        private readonly ConcurrentDictionary<Type, IDictionary<Type, Delegate>> _handlerList;

        private readonly ConcurrentDictionary<MessageContext, byte> _currentMessages = new ConcurrentDictionary<MessageContext, byte>();

        private ActionBlock<string> _receiverBlock;
        private ActionBlock<IHandlerExecutionContext> _parallelProcessingBlock;
        private ActionBlock<IHandlerExecutionContext> _throttledProcessingBlock;
        private BufferBlock<IHandlerExecutionContext> _parallelProcessingInputBlock;

        private Timer _renewTimer;

        private string _queueUrl;

        public MessageBus(IOptions<AWSMessageBusConfig> config,
            ILoggerFactory loggerFactory,
            IFilterProvider filterProvider,
            IMessageSerializer serializer)
            : this(loggerFactory,
                new AmazonSQSClient(config.Value?.Region ?? throw new ArgumentNullException(nameof(config))),
                new AmazonSimpleNotificationServiceClient(config.Value?.Region ?? throw new ArgumentNullException(nameof(config))),
                filterProvider,
                serializer,
                config)
        {
        }

        private MessageBus(ILoggerFactory loggerFactory,
            IAmazonSQS sqsClient,
            IAmazonSimpleNotificationService snsClient,
            IFilterProvider filterProvider,
            IMessageSerializer serializer,
            IOptions<AWSMessageBusConfig> config)
        {
            this._filterProvider = filterProvider ?? new EmptyFilterProvider();

            this._logger = loggerFactory?.CreateLogger<MessageBus>() ??
                           throw new ArgumentNullException(nameof(loggerFactory));

            this._sqsClient = sqsClient ?? throw new ArgumentNullException(nameof(sqsClient));
            this._snsClient = snsClient ?? throw new ArgumentNullException(nameof(snsClient));
            this._serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));

            this._config = config?.Value ?? throw new ArgumentNullException(nameof(config));

#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            if (string.IsNullOrWhiteSpace(this._config.QueueName))
            {
                throw new ArgumentNullException(nameof(this._config.QueueName));
            }

            if (string.IsNullOrWhiteSpace(this._config.TopicName))
            {
                throw new ArgumentNullException(nameof(this._config.TopicName));
            }
#pragma warning restore CA2208 // Instantiate argument exceptions correctly

            this._handlerList = new ConcurrentDictionary<Type, IDictionary<Type, Delegate>>();

            this._maxParallellCapacity = this._config.ParallellBlockSize ?? Math.Max(Environment.ProcessorCount, 4) * 2;

            this._maxThrottledCapacity = this._config.ThrottledBlockSize ?? 1;
        }

        public void Subscribe<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            this.SubscribeAsync(factory).GetAwaiter().GetResult();
        }

        public Task SubscribeAsync<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory, CancellationToken cancellationToken = default(CancellationToken))
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            try
            {
                this._semaphore.WaitOne();

                var messageType = typeof(TMessage);
                if (this._handlerList.ContainsKey(messageType) == false)
                {
                    this._handlerList.TryAdd(messageType, new Dictionary<Type, Delegate>());
                }

                var bucket = this._handlerList[messageType];

                var handlerType = typeof(THandler);
                if (bucket.ContainsKey(handlerType))
                {
                    this._logger.LogWarning($"Handler {handlerType} has been subscribed to the {messageType} already", handlerType.Name, messageType.Name);
                    return Task.CompletedTask;
                }

                bucket.Add(handlerType, factory);
                this._logger.LogInformation($"Handler {handlerType} has been subscribed to the {messageType}", handlerType.Name, messageType.Name);
            }
            finally
            {
                this._semaphore.Release();
            }

            return Task.CompletedTask;
        }

        public void Publish<T>(T message)
            where T : IMessage, new()
        {
            this.PublishAsync(message).Wait();
        }

        public async Task PublishAsync<T>(T message, CancellationToken cancellationToken = default(CancellationToken))
            where T : IMessage, new()
        {
            if (message == null)
            {
                await Task.FromException(new ArgumentNullException(nameof(message))).ConfigureAwait(false);
                return;
            }

            var filterContext = new FilterExecutionContext();
            await this._filterProvider.GetFilters(message).ApplySendProcessingAsync(message, filterContext, async () =>
            {
                try
                {
                    var createTopicResponse = await this._snsClient
                        .CreateTopicAsync(new CreateTopicRequest(this._config.TopicName), cancellationToken)
                        .ConfigureAwait(false);
                    if (createTopicResponse.HttpStatusCode == HttpStatusCode.OK)
                    {
                        var topicArn = createTopicResponse.TopicArn;
                        var publishResponse = await this._snsClient.PublishAsync(new PublishRequest
                        {
                            TopicArn = topicArn,
                            Message = await this._serializer.SerializeMessageAsync(message, cancellationToken).ConfigureAwait(false),
                            MessageAttributes = new Dictionary<string, MessageAttributeValue>
                            {
                                {
                                    "ClrType", new MessageAttributeValue
                                    {
                                        DataType = "String",
                                        StringValue = typeof(T).ToMessageType(),
                                    }
                                },
                                {
                                    "Properties", new MessageAttributeValue
                                    {
                                        DataType = "String",
                                        StringValue = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(filterContext.Metadata))),
                                    }
                                },
                            },
                        }, cancellationToken).ConfigureAwait(false);

                        if (publishResponse.HttpStatusCode == HttpStatusCode.OK)
                        {
                            this._logger.LogInformation("Message {messageType} has been published, {meta}", typeof(T).Name, filterContext.Metadata);
                        }
                    }
                }
                catch (AmazonSimpleNotificationServiceException ex)
                {
                    this._logger.LogError(ex, ex.Message);
                    throw;
                }
                catch (OperationCanceledException ex)
                {
                    this._logger.LogError(ex, ex.Message);
                    throw;
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, ex.Message);
                    throw;
                }
            }).ConfigureAwait(false);
        }

        public async Task StartListeningAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this._handlerList.Any() == false)
            {
                return;
            }

            if (this._semaphore.WaitOne(TimeSpan.FromMilliseconds(500)) == false)
            {
                this._logger.LogWarning($"Listening has been started already");
                return;
            }

            try
            {
                var createTopicResponse = await this._snsClient
                    .CreateTopicAsync(new CreateTopicRequest(this._config.TopicName), cancellationToken)
                    .ConfigureAwait(false);
                if (createTopicResponse.HttpStatusCode == HttpStatusCode.OK)
                {
                    this._logger.LogInformation("Topic {topicName} has been resolved. Topic arn: {topicArn}", this._config.TopicName, createTopicResponse.TopicArn);

                    try
                    {
                        var createQueueResponse = await this._sqsClient.CreateQueueAsync(
                            new CreateQueueRequest(this._config.QueueName)
                            {
                                Attributes = new Dictionary<string, string>
                                {
                                    {
                                        "VisibilityTimeout", (this._messageHandlingTimeout.TotalSeconds + 2).ToString(CultureInfo.InvariantCulture)
                                    },
                                },
                            }, cancellationToken).ConfigureAwait(false);

                        this._queueUrl = createQueueResponse.HttpStatusCode == HttpStatusCode.OK
                            ? createQueueResponse.QueueUrl
                            : null;
                    }
                    catch (AmazonSQSException e) when (e.StatusCode == HttpStatusCode.BadRequest &&
                                                       e.ErrorCode == "QueueAlreadyExists")
                    {
                        this._logger.LogWarning("Queue {queue} already exists with different settings.", this._config.QueueName);

                        var response = await this._sqsClient.GetQueueUrlAsync(this._config.QueueName, cancellationToken)
                            .ConfigureAwait(false);

                        this._queueUrl = response.QueueUrl;
                    }

                    if (string.IsNullOrWhiteSpace(this._queueUrl) == false)
                    {
                        this._logger.LogInformation("Queue {queueName} has been resolved. Queue url: {queueUrl}", this._config.QueueName, this._queueUrl);

                        var subscriptionArn = await this._snsClient
                            .SubscribeQueueAsync(createTopicResponse.TopicArn, this._sqsClient, this._queueUrl)
                            .ConfigureAwait(false);

                        this._logger.LogInformation("Subscribed queue {queueName} to topic {topicName}. Subscription arn: {subscriptionArn}", this._config.QueueName, this._config.TopicName, subscriptionArn);

                        var filter = JsonConvert.SerializeObject(new
                        {
                            ClrType = this._handlerList.Keys.Where(x => typeof(IServiceMessage).IsAssignableFrom(x) == false)
                                                            .Select(x => x.ToMessageType())
                                                            .OrderBy(x => x),
                        });

                        await this._snsClient
                            .SetSubscriptionAttributesAsync(subscriptionArn, "FilterPolicy", filter, cancellationToken)
                            .ConfigureAwait(false);

                        this._renewTimer = new Timer(this.RenewVisibility, null, TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(10));

                        this.InitializeProcessingBlocks(cancellationToken);

                        this._receiverBlock.Post(this._queueUrl);

                        this._logger.LogInformation("Started listening queue for {handlerCount} handlers", this._handlerList.Count);
                    }
                }
            }
            catch (AmazonSimpleNotificationServiceException ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
            catch (AmazonSQSException ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
            catch (OperationCanceledException ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
            finally
            {
                this._semaphore.Release();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                this._renewTimer?.Dispose();

                this._handlerList?.Clear();

                if (this._semaphore.WaitOne(TimeSpan.FromSeconds(5)) == false)
                {
                    // dispose with force
                }

                this._semaphore.Dispose();

                this._sqsClient?.Dispose();
                this._snsClient?.Dispose();
            }
        }

        private static ServiceEvent ConvertToArgs(MessageContext context)
        {
            return new ServiceEvent
            {
                Message = context.Message,
                Metadata = context.Metadata,
                ReceiveTimestamp = context.ReceiveTimestamp,
                ReceivedCount = context.ReceivedCount,
            };
        }

        private static ServiceEvent ConvertToArgs(IHandlerExecutionContext context)
        {
            return new ServiceEvent()
            {
                Message = context.MessageContext.Message,
                Metadata = context.MessageContext.Metadata,
                ReceiveTimestamp = context.MessageContext.ReceiveTimestamp,
                ReceivedCount = context.MessageContext.ReceivedCount,
                HandlerData = new HandlerData
                {
                    Handler = context.Handler,
                    HandlerStartedTimestamp = context.HandlerStartedTimestamp,
                    Options = context.Options,
                },
            };
        }

        private void RenewVisibility(object state)
        {
            var snapshot = this._currentMessages.Keys.ToList();

            foreach (var currentMessage in snapshot)
            {
                try
                {
                    if (currentMessage.Handlers.Any())
                    {
                        var inProcessing = DateTime.UtcNow - currentMessage.ReceiveTimestamp;

                        var maxProcessing = currentMessage.Handlers.Max(x => x.Key.Options?.ExecutionTimeout) ??
                                            this._messageHandlingTimeout;

                        if ((currentMessage.CurrentVisibility - maxProcessing).TotalSeconds < 3
                            && (currentMessage.CurrentVisibility - inProcessing).TotalSeconds < 17)
                        {
                            var newVisibility = inProcessing.TotalSeconds + 17;

                            this._sqsClient.ChangeMessageVisibilityAsync(
                                    this._queueUrl,
                                    currentMessage.QueueMessage.ReceiptHandle,
                                    (int)newVisibility,
                                    CancellationToken.None)
                                .GetAwaiter()
                                .GetResult();

                            currentMessage.LastRenewTimestamp = DateTime.UtcNow;
                            currentMessage.CurrentVisibility = TimeSpan.FromSeconds(newVisibility);

                            this._logger.LogInformation(
                                "Visibility timeout changed to {newTimeout} for message of type {messageType}. Current processing time {processingTime}.",
                                currentMessage.CurrentVisibility,
                                currentMessage.MessageType,
                                inProcessing);
                        }
                    }
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception e)
                {
                    this._logger.LogWarning(e, "Unable to renew message visibility timeout.");
                }
#pragma warning restore CA1031 // Do not catch general exception types
            }
        }

        private void InitializeProcessingBlocks(CancellationToken cancellationToken)
        {
            this._logger.LogInformation(
                "Creating processing data block for parallell jobs with BoundedCapacity {maxProcessingCapacity}",
                this._maxParallellCapacity);

            this._parallelProcessingBlock = new ActionBlock<IHandlerExecutionContext>(
                async args => await this.ExecuteHandlerAsync(args, cancellationToken).ConfigureAwait(false),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = this._maxParallellCapacity / 2,
                    BoundedCapacity = -1,
                    CancellationToken = cancellationToken,
                });

            this._parallelProcessingInputBlock = new BufferBlock<IHandlerExecutionContext>(
                new DataflowBlockOptions
                {
                    CancellationToken = cancellationToken,
                    BoundedCapacity = this._maxParallellCapacity,
                });

            this._logger.LogInformation(
                "Creating processing data block for long running jobs with BoundedCapacity {maxProcessingCapacity}", 1);

            this._throttledProcessingBlock = new ActionBlock<IHandlerExecutionContext>(
                async args => await this.ExecuteHandlerAsync(args, cancellationToken).ConfigureAwait(false),
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = this._maxThrottledCapacity,
                    BoundedCapacity = this._maxThrottledCapacity,
                    CancellationToken = cancellationToken,
                });

            this._parallelProcessingInputBlock.LinkTo(this._parallelProcessingBlock);

            var totalCapacity = this._maxParallellCapacity + this._maxThrottledCapacity;
            this._receiverBlock = new ActionBlock<string>(async queueName =>
            {
                if (await this.PullQueueAsync(queueName, cancellationToken).ConfigureAwait(false) == false ||
                    totalCapacity - (this._parallelProcessingBlock.InputCount +
                                     this._throttledProcessingBlock.InputCount) > 0)
                {
                    this._receiverBlock.Post(queueName);
                }
            }, new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                BoundedCapacity = -1,
                MaxDegreeOfParallelism = 1,
            });
        }

        private async Task<bool> PullQueueAsync(string queueUrl, CancellationToken cancellationToken = default(CancellationToken))
        {
            ReceiveMessageResponse receiveResult = null;

            try
            {
                var emptySlotsCount = (this._maxParallellCapacity + this._maxThrottledCapacity) -
                                      (this._throttledProcessingBlock.InputCount +
                                       this._parallelProcessingBlock.InputCount);

                if (emptySlotsCount <= 0)
                {
                    return false;
                }

                receiveResult = await this._sqsClient.ReceiveMessageAsync(new ReceiveMessageRequest
                {
                    MaxNumberOfMessages = Math.Min(emptySlotsCount, 10),
                    QueueUrl = queueUrl,
                    WaitTimeSeconds = 20,
                    AttributeNames = new List<string> { "All" },
                }, cancellationToken).ConfigureAwait(false);
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (cancellationToken.IsCancellationRequested || receiveResult == null ||
                receiveResult.HttpStatusCode != HttpStatusCode.OK || receiveResult.Messages.Any() == false)
            {
                return false;
            }

            var messageReceivedAt = DateTime.UtcNow;

            foreach (var queueMessage in receiveResult.Messages)
            {
                var json = JObject.Parse(queueMessage.Body);

                var messageTypeName = json["MessageAttributes"]["ClrType"]["Value"].ToObject<string>();

                Type messageType;

                try
                {
                    messageType = Type.GetType(messageTypeName, true);
                }
#pragma warning disable CA1031 // Do not catch general exception types
                catch (Exception ex)
                {
                    this._logger.LogError(ex, ex.Message + "message type name: " + messageTypeName);
                    return false;
                }
#pragma warning restore CA1031 // Do not catch general exception types

                var message = await _serializer.DeserializeMessageAsync(json["Message"].ToObject<string>(), messageType, cancellationToken).ConfigureAwait(false);

                if (message == null || this._handlerList.ContainsKey(messageType) == false)
                {
                    return false;
                }

                var iMessage = (IMessage)message;

                Dictionary<string, string> messageProperties;

                try
                {
                    var base64 = json["MessageAttributes"]?["Properties"]?["Value"]?.ToObject<string>();
                    if (string.IsNullOrWhiteSpace(base64) == false)
                    {
                        var bytes = Convert.FromBase64String(base64);
                        var propertiesString = Encoding.UTF8.GetString(bytes);
                        messageProperties = JsonConvert.DeserializeObject<Dictionary<string, string>>(propertiesString);
                    }
                    else
                    {
                        messageProperties = new Dictionary<string, string>();
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, "Unable to read message properties for message  {messageType}. ", messageType);
                    return false;
                }

                var filterContext = new FilterExecutionContext(messageProperties);

                try
                {
                    var posted = false;

                    await this._filterProvider.GetFilters(iMessage).ApplyReceiveProcessingAsync(iMessage, filterContext, async () =>
                        {
                            var messageContext = new MessageContext(messageReceivedAt, iMessage, messageType, queueMessage, filterContext.Metadata);

                            var isDead = await this.DeadLetterMessageAsync(messageContext, cancellationToken).ConfigureAwait(false);
                            if (isDead)
                            {
                                await this.OnDeadMessageAsync(messageContext, cancellationToken).ConfigureAwait(false);

                                return;
                            }

                            this._logger.LogInformation("Message {MessageType} has been received, {@message}", messageContext.MessageType.Name, (IMessage)messageContext.Message);

                            await this.OnReceiveMessageAsync(messageContext, cancellationToken).ConfigureAwait(false);

                            foreach (var item in this._handlerList[messageType])
                            {
                                try
                                {
                                    var handlerScope = (IHandlerScope)item.Value.DynamicInvoke();

                                    if (handlerScope == null)
                                    {
                                        return;
                                    }

                                    var type = typeof(HandlerExecutionContext<,>).MakeGenericType(iMessage.GetType(), handlerScope.GetType().GetGenericArguments()[1]);
                                    var context = Activator.CreateInstance(type, messageContext, handlerScope) as IHandlerExecutionContext;

                                    var handlerContext = messageContext.AttachHandler(context);

                                    await this._filterProvider.GetFilters(iMessage).ApplyHandleProcessingAsync(iMessage, handlerScope.HandleOptions, filterContext, async () =>
                                    {
                                        if (this.TryPostToActionBlock(handlerContext))
                                        {
                                            await this.OnPostedToBlockAsync(handlerContext, cancellationToken).ConfigureAwait(false);

                                            posted = true;
                                        }
                                        else
                                        {
                                            await this.OnNoProcessingBlocksAsync(handlerContext, cancellationToken).ConfigureAwait(false);

                                            messageContext.DetachHandler(handlerContext);
                                        }
                                    }).ConfigureAwait(false);
                                }
                                catch (Exception ex)
                                {
                                    this._logger.LogError(ex, "Error processing messages");
                                }
                            }

                            if (messageContext.Handlers.Any())
                            {
                                this._currentMessages.TryAdd(messageContext, 0);
                            }
                        }).ConfigureAwait(false);

                    return posted;
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Message processing error");
                    return false;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();

            return false;
        }

        private async Task<bool> DeadLetterMessageAsync(MessageContext messageContext, CancellationToken cancellationToken)
        {
            if (messageContext.ReceivedCount > this._config.MaxReceiveCount)
            {
                this._logger.LogWarning("Message reached max receive count {MaxReceiveCount} and will be removed without processing", this._config.MaxReceiveCount);
                try
                {
                    await this._sqsClient.DeleteMessageAsync(this._queueUrl, messageContext.QueueMessage.ReceiptHandle, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Unable to delete dead message");
                    throw;
                }

                return true;
            }

            return false;
        }

        private bool TryPostToActionBlock(IHandlerExecutionContext handlerContext)
        {
            void UpdateVisibility()
            {
                var additionalTime = (handlerContext.MessageContext.ReceivedCount * 10) + 15;

                var absoluteTime = (DateTime.UtcNow - handlerContext.MessageContext.ReceiveTimestamp).TotalSeconds + additionalTime;

                this._sqsClient.ChangeMessageVisibilityAsync(this._queueUrl, handlerContext.MessageContext.QueueMessage.ReceiptHandle, (int)absoluteTime);

                handlerContext.MessageContext.LastRenewTimestamp = DateTime.UtcNow;
                handlerContext.MessageContext.CurrentVisibility = TimeSpan.FromSeconds(absoluteTime);
            }

            var messageType = handlerContext.MessageContext.MessageType;

            if (handlerContext.Options?.DisableParallelProcessing == true)
            {
                var result = this._throttledProcessingBlock.Post(handlerContext);
                if (result)
                {
                    this._logger.LogInformation("Throttled message processing block for {message} was posted.", messageType);
                    return true;
                }

                UpdateVisibility();

                this._logger.LogWarning("Throttled message processing block for {message} was not posted. No processing slots available.", messageType);
            }
            else
            {
                var result = this._parallelProcessingInputBlock.Post(handlerContext);

                if (result)
                {
                    this._logger.LogInformation("Parallel message processing block for {message} was posted", messageType);
                    return true;
                }

                UpdateVisibility();

                this._logger.LogWarning("Parallel message processing block for {message} was not posted. No processing slots available.", messageType);
            }

            return false;
        }

        private async Task ExecuteHandlerAsync(IHandlerExecutionContext handlerContext, CancellationToken cancellationToken)
        {
            var handlerCancellation = new CancellationTokenSource();
            var timeoutCancellation = new CancellationTokenSource();

            try
            {
                cancellationToken.Register(handlerCancellation.Cancel);

                cancellationToken.Register(timeoutCancellation.Cancel);

                var timeout = handlerContext.Options?.ExecutionTimeout ?? this._messageHandlingTimeout;

                handlerContext.HandlerStartedTimestamp = DateTime.UtcNow;

                await this.OnStartingAsync(handlerContext, cancellationToken).ConfigureAwait(false);

                var handlerTask = ((Task)handlerContext.ExecuteAsync(handlerContext.MessageContext.Message, cancellationToken))
                    .ContinueWith<Task>(async task =>
                    {
                        var msgType = handlerContext.MessageContext.MessageType.Name;
                        var iMsg = (IMessage)handlerContext.MessageContext.Message;
                        var deleteMessage = true;

                        if (task.IsCanceled)
                        {
                            deleteMessage = await this.OnExceptionAsync(handlerContext, task.Exception, cancellationToken).ConfigureAwait(false);

                            this._logger.LogInformation("Message {messageType} has been cancelled. {@message}", msgType, iMsg);
                        }
                        else if (task.IsFaulted)
                        {
                            deleteMessage = await this.OnExceptionAsync(handlerContext, task.Exception, cancellationToken).ConfigureAwait(false);

                            this._logger.LogError(task.Exception, "Message {messageType} finished with error. {@message}", msgType, iMsg);
                        }
                        else
                        {
                            await this.OnSuccessAsync(handlerContext, cancellationToken).ConfigureAwait(false);
                            this._logger.LogInformation("Message {messageType} has been processed. {@message}", msgType, iMsg);
                        }

                        handlerContext.Dispose();

                        if (deleteMessage == true)
                        {
                            try
                            {
                                if (handlerContext.MessageContext.Handlers.Any() == false)
                                {
                                    await this._sqsClient.DeleteMessageAsync(new DeleteMessageRequest
                                    {
                                        QueueUrl = this._queueUrl,
                                        ReceiptHandle = handlerContext.MessageContext.QueueMessage.ReceiptHandle,
                                    }, cancellationToken).ConfigureAwait(false);
                                }
                            }
                            catch (AmazonSQSException ex)
                            {
                                this._logger.LogError(ex, ex.Message);
                            }
                            catch (OperationCanceledException ex)
                            {
                                this._logger.LogError(ex, ex.Message);
                                throw;
                            }
                            catch (Exception ex)
                            {
                                this._logger.LogError(ex, ex.Message);
                            }
                        }

                        if (handlerContext.MessageContext.Handlers.Any() == false)
                        {
                            this._currentMessages.TryRemove(handlerContext.MessageContext, out _);
                        }
                    }, TaskScheduler.Current);

                var timeoutTask = Task.Delay(timeout, timeoutCancellation.Token).ContinueWith(task =>
                {
                    handlerCancellation.Cancel();

                    if (task.IsCanceled == false)
                    {
                        this._logger.LogWarning("Handler {handler} execution timeout {timeout} reached.", handlerContext.Handler.GetType().Name, timeout);
                    }
                }, TaskScheduler.Current);

                await Task.WhenAny(handlerTask, timeoutTask).ConfigureAwait(false);

                timeoutCancellation.Cancel();

                this._logger.LogInformation("Pulled message {messageType} has been processed.", handlerContext.MessageContext.MessageType.Name);
            }
            catch (OperationCanceledException ex)
            {
                this._logger.LogError(ex, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message);
            }
            finally
            {
                handlerCancellation.Dispose();
                timeoutCancellation.Dispose();
            }

            this._receiverBlock.Post(this._queueUrl);
        }

        private async Task OnDeadMessageAsync(MessageContext messageContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(messageContext);
            await this.ExecuteServiceHandlersAsync(new DeadMessageEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnReceiveMessageAsync(MessageContext messageContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(messageContext);
            await this.ExecuteServiceHandlersAsync(new MessageReceivedEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnPostedToBlockAsync(IHandlerExecutionContext handlerContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(handlerContext);
            await this.ExecuteServiceHandlersAsync(new HandlerPostedForProcessingEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnNoProcessingBlocksAsync(IHandlerExecutionContext handlerContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(handlerContext);
            await this.ExecuteServiceHandlersAsync(new HandlerDiscardedEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task OnStartingAsync(IHandlerExecutionContext handlerContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(handlerContext);
            await this.ExecuteServiceHandlersAsync(new HandlerStartedEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task<bool> OnExceptionAsync(IHandlerExecutionContext handlerContext, Exception exception, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(handlerContext);

            var evt = new HandlerExceptionEvent { EventArgs = args, Exception = exception };

            await this.ExecuteServiceHandlersAsync(evt, cancellationToken).ConfigureAwait(false);

            return evt.Handled;
        }

        private async Task OnSuccessAsync(IHandlerExecutionContext handlerContext, CancellationToken cancellationToken)
        {
            var args = ConvertToArgs(handlerContext);
            await this.ExecuteServiceHandlersAsync(new HandlerSuccessEvent { EventArgs = args }, cancellationToken).ConfigureAwait(false);
        }

        private async Task ExecuteServiceHandlersAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
            where TMessage : IServiceMessage
        {
            try
            {
                if (this._handlerList.ContainsKey(typeof(TMessage)) == false)
                {
                    return;
                }

                foreach (var factory in this._handlerList[typeof(TMessage)])
                {
                    using (dynamic handlerScope = factory.Value.DynamicInvoke())
                    {
                        await handlerScope.Handler.HandleAsync(message, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Error executing service handler for event type {eventType}", typeof(TMessage));
                throw;
            }
        }
    }
}