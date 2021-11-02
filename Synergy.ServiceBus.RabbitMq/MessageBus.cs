using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Synergy.ServiceBus.Abstracts;
using Synergy.ServiceBus.Abstracts.Filtering;
using Synergy.ServiceBus.Abstracts.ServiceEvents;
using Synergy.ServiceBus.RabbitMq.Extensions;

namespace Synergy.ServiceBus.RabbitMq
{
    public class MessageBus : IMessageBus, IDisposable
    {
        private readonly Semaphore _semaphore = new Semaphore(1, 1);

        private readonly ILogger _logger;
        private readonly IConnectionFactory _connectionFactory;
        private readonly ConcurrentDictionary<Type, IDictionary<Type, Delegate>> _handlerList;
        private readonly IFilterProvider _filterProvider;

        private readonly Lazy<IConnection> _inputConnection;
        private readonly Lazy<IModel> _inputChannel;
        private readonly TimeSpan _messageHandlingTimeout = TimeSpan.FromMinutes(2);

        private string _exchange;
        private string _queue;

        public MessageBus(IOptions<RabbitMQConfig> config,
            ILogger<MessageBus> logger,
            IFilterProvider filterProvider = null)
            : this(logger,
                  new ConnectionFactory()
                  {
                      HostName = config.Value?.Host ?? throw new ArgumentNullException(nameof(config)),
                      Port = config.Value?.Port ?? throw new ArgumentNullException(nameof(config)),
                      DispatchConsumersAsync = true,
                      UserName = config.Value?.User ?? throw new ArgumentNullException(nameof(config)),
                      Password = config.Value?.Password ?? throw new ArgumentNullException(nameof(config)),
                  },
                  filterProvider)
        {
        }

        private MessageBus(ILogger<MessageBus> logger, IConnectionFactory connectionFactory, IFilterProvider filterProvider = null)
        {
            this._logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this._connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            this._filterProvider = filterProvider ?? new EmptyFilterProvider();
            this._handlerList = new ConcurrentDictionary<Type, IDictionary<Type, Delegate>>();

            this._inputConnection = new Lazy<IConnection>(this.CreateConnectionWithRetries);
            this._inputChannel = new Lazy<IModel>(this.InputConnection.CreateModel);
        }

        protected IConnection InputConnection => this._inputConnection.Value;

        protected IModel InputChannel => this._inputChannel.Value;

        protected string Exchange
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._exchange))
                {
                    this._exchange = "synergy";
                    this.InputChannel.ExchangeDeclare(this._exchange, "direct", true, false, null);
                }

                return this._exchange;
            }
        }

        protected string Queue
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this._queue))
                {
                    this._queue = Assembly.GetEntryAssembly().GetName().Name;
                    this.InputChannel.QueueDeclare(this._queue, true, false, false, null);
                }

                return this._queue;
            }
        }

        public void Subscribe<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory)
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            this.SubscribeAsync(factory).GetAwaiter().GetResult();
        }

        public async Task SubscribeAsync<TMessage, THandler>(Func<IHandlerScope<TMessage, THandler>> factory, CancellationToken cancellationToken = default(CancellationToken))
            where TMessage : IMessage
            where THandler : IMessageHandler<TMessage>
        {
            if (factory == null)
            {
                throw new ArgumentNullException(nameof(factory));
            }

            await Task.Yield();

            try
            {
                this._semaphore.WaitOne();

                var messageType = typeof(TMessage);
                if (this._handlerList.ContainsKey(messageType) == false)
                {
                    var routingKey = messageType.FullName?.ToLowerInvariant();
                    try
                    {
                        this.InputChannel.QueueBind(this.Queue, this.Exchange, routingKey, null);
                        this._handlerList.TryAdd(messageType, new Dictionary<Type, Delegate>());
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, ex.Message);
                        return;
                    }
                }

                var bucket = this._handlerList[messageType];

                var handlerType = typeof(THandler);
                if (bucket.ContainsKey(handlerType))
                {
                    this._logger.LogWarning($"Handler {handlerType} has been subscribed to the {messageType} already", handlerType.Name, messageType.Name);
                    return;
                }

                bucket.Add(handlerType, factory);
                this._logger.LogInformation($"Handler {handlerType} has been subscribed to the {messageType}", handlerType.Name, messageType.Name);
            }
            finally
            {
                this._semaphore.Release();
            }

            return;
        }

        public void Publish<T>(T message)
            where T : IMessage, new()
        {
            this.PublishAsync(message, CancellationToken.None).Wait();
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
            await this._filterProvider.GetFilters(message).ApplySendProcessingAsync(message, filterContext, () =>
            {
                var messageType = typeof(T);
                var routingKey = messageType.FullName?.ToLowerInvariant();
                var data = message.SerializeMessage();

                try
                {
                    using (var connection = this._connectionFactory.CreateConnection())
                    using (var model = connection.CreateModel())
                    {
                        var properties = model.CreateBasicProperties();
                        properties.ContentType = "application/json";
                        properties.DeliveryMode = 2;
                        properties.Persistent = true;
                        properties.Headers = new Dictionary<string, object>
                        {
                            { "ClrType", messageType.ToMessageType() },
                            { "Properties", JsonConvert.ToString(JsonConvert.SerializeObject(filterContext.Metadata)) },
                        };

                        model.BasicPublish(exchange: this.Exchange, routingKey: routingKey, mandatory: false, basicProperties: properties, body: data);

                        this._logger.LogInformation("Message {messageType} has been published", message.GetType());
                    }
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, ex.Message);
                    throw;
                }

                return Task.CompletedTask;
            }).ConfigureAwait(false);
        }

        public Task StartListeningAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            if (this._handlerList.Any() == false)
            {
                return Task.CompletedTask;
            }

            var consumer = new AsyncEventingBasicConsumer(this.InputChannel);

            ActionBlock<(BasicDeliverEventArgs, CancellationToken)> processingBlock = new ActionBlock<(BasicDeliverEventArgs message, CancellationToken cancellation)>(
                args => this.ProcessQueueMessageAsync(args.message, args.cancellation).ConfigureAwait(false),
                new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = Math.Max(Environment.ProcessorCount, 4) });

            consumer.Received += (_, @event) =>
            {
                processingBlock.Post((@event, cancellationToken));

                return Task.CompletedTask;
            };

            this.InputChannel.BasicConsume(this.Queue, false, string.Empty, false, false, null, consumer);

            return Task.CompletedTask;
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
                this._handlerList.Clear();

                if (this._inputChannel.IsValueCreated)
                {
                    this.InputChannel.Dispose();
                }

                if (this._inputConnection.IsValueCreated)
                {
                    this._inputConnection.Value.Dispose();
                }

                if (this._semaphore.WaitOne(TimeSpan.FromSeconds(5)) == false)
                {
                    // dispose with force
                }

                this._semaphore.Dispose();
            }
        }

        private async Task ProcessQueueMessageAsync(BasicDeliverEventArgs @event, CancellationToken cancellationToken)
        {
            TimeSpan timeout;

            if (@event.BasicProperties.Headers.ContainsKey("ClrType") == false)
            {
                this._logger.LogError("No ClrType header found");
                return;
            }

            var messageTypeName = Encoding.UTF8.GetString((byte[])@event.BasicProperties.Headers["ClrType"]);

            Type messageType;
            try
            {
                messageType = Type.GetType(messageTypeName);
            }
            catch (Exception ex)
            {
                this._logger.LogError(ex, ex.Message, messageTypeName);
                return;
            }

            dynamic message = @event.Body.DeserializeMessage(messageType);
            if (messageType == null || message == null || this._handlerList.ContainsKey(messageType) == false)
            {
                this._logger.LogError("No hendlers found");
                return;
            }

            var properties = new Dictionary<string, string>();

            try
            {
                if (@event.BasicProperties.Headers.ContainsKey("Properties"))
                {
                    var propertiesJson = JToken.Parse(Encoding.UTF8.GetString((byte[])@event.BasicProperties.Headers["Properties"])).ToString();
                    properties = JsonConvert.DeserializeObject<Dictionary<string, string>>(propertiesJson);
                }
            }
            catch (Exception e)
            {
                this._logger.LogError(e, "Error parsing message properties. Processing of this message terminated.");
                return;
            }

            var filterContext = new FilterExecutionContext(properties);

            var iMessage = (IMessage)message;

            await this._filterProvider.GetFilters(iMessage).ApplyReceiveProcessingAsync(iMessage, filterContext, async () =>
            {
                await this.SafeExecuteServiceHandlersAsync(new MessageReceivedEvent { EventArgs = new ServiceEvent() { Message = iMessage, Metadata = properties, ReceiveTimestamp = DateTime.UtcNow } }, cancellationToken).ConfigureAwait(false);

                var tasks = this._handlerList[messageType].Select(async item =>
                {
                    try
                    {
                        dynamic handlerScope = item.Value.DynamicInvoke();

                        timeout = handlerScope.HandleOptions?.ExecutionTimeout ?? this._messageHandlingTimeout;

                        var handlerCancellation = new CancellationTokenSource();
                        cancellationToken.Register(handlerCancellation.Cancel);

                        var timeoutCancellation = new CancellationTokenSource();
                        cancellationToken.Register(timeoutCancellation.Cancel);

                        var handler = handlerScope.Handler;

                        foreach (var filter in this._filterProvider.GetFilters((IMessage)message))
                        {
                            await filter.PreHandleMessageAsync((IMessage)message, handlerScope.HandleOptions, filterContext);
                        }

                        var handlerTask = ((Task)handler.HandleAsync(message, cancellationToken))
                            .ContinueWith(async task =>
                            {
                                var eventArgs = new ServiceEvent()
                                {
                                    Message = iMessage,
                                    Metadata = properties,
                                    ReceiveTimestamp = DateTime.UtcNow,
                                    HandlerData = new HandlerData()
                                    {
                                        Handler = (IMessageHandler)handler,
                                        HandlerStartedTimestamp = DateTime.UtcNow,
                                        Options = handlerScope.HandleOptions,
                                    },
                                };

                                if (task.IsCanceled)
                                {
                                    await this.SafeExecuteServiceHandlersAsync(new HandlerExceptionEvent() { EventArgs = eventArgs, Exception = task.Exception }, cancellationToken).ConfigureAwait(false);

                                    this._logger.LogInformation("Message {messageType} has been cancelled. {message}", messageType.Name, iMessage);
                                }
                                else if (task.IsFaulted)
                                {
                                    await this.SafeExecuteServiceHandlersAsync(new HandlerExceptionEvent() { EventArgs = eventArgs, Exception = task.Exception }, cancellationToken).ConfigureAwait(false);

                                    this._logger.LogError(task.Exception, "Message {messageType} finished with error. {message}", messageType.Name, iMessage);
                                }
                                else if (task.IsCompleted)
                                {
                                    await this.SafeExecuteServiceHandlersAsync(new HandlerSuccessEvent() { EventArgs = eventArgs }, cancellationToken).ConfigureAwait(false);

                                    this._logger.LogInformation("Message {messageType} has been processed. {message}", messageType.Name, iMessage);

                                    this.InputChannel.BasicAck(@event.DeliveryTag, false);
                                }

                                handlerScope.Dispose();

                                return;
                            }, TaskScheduler.Current);

                        var timeoutTask = Task.Delay(timeout, timeoutCancellation.Token)
                            .ContinueWith(
                                task =>
                                {
                                    handlerCancellation.Cancel();

                                    if (task.IsCanceled == false)
                                    {
                                        string type = handlerScope.Handler.GetType().Name;
                                        this._logger.LogWarning("Handler {handler} execution timeout {timeout} reached.", type, timeout);
                                    }
                                }, TaskScheduler.Current);

                        await Task.WhenAny(handlerTask, timeoutTask).ConfigureAwait(false);

                        timeoutCancellation.Cancel();

                        this._logger.LogInformation("Pulled message {messageType} has been processed.", messageType.Name);
                    }
                    catch (OperationCanceledException ex)
                    {
                        this._logger.LogError(ex, ex.Message);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        this._logger.LogError(ex, ex.Message, iMessage);
                    }
                });

                try
                {
                    await Task.WhenAll(tasks.ToArray()).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    this._logger.LogError(ex, ex.Message, iMessage);
                }
            }).ConfigureAwait(false);
        }

        // If the connection attempt failed will retry connection procedure up to 10 times.
        // Useful when running in docker compose and RabbitMQ instance is starting after consumer application
        private IConnection CreateConnectionWithRetries()
        {
            var maxConnectAttempts = 10;
            var connectAttempts = 1;
            IConnection connectionInstance = null;

            var exceptions = new List<Exception>();
            while (connectionInstance == null && connectAttempts <= maxConnectAttempts)
            {
                try
                {
                    connectionInstance = this._connectionFactory.CreateConnection();
                }
                catch (Exception e)
                {
                    this._logger.LogError(e, "Unable to connect to the RabbitMQ server. Connection attempt {connectAttempts} of {maxConnectAttempts}.", connectAttempts, maxConnectAttempts);
                    exceptions.Add(e);

                    Task.Delay(TimeSpan.FromSeconds(5 * connectAttempts)).Wait();
                }

                connectAttempts++;
            }

            if (connectionInstance == null)
            {
                throw new AggregateException("Maximum connection retries riched", exceptions);
            }

            return connectionInstance;
        }

        private async Task SafeExecuteServiceHandlersAsync<TMessage>(TMessage message, CancellationToken cancellationToken)
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
                        await handlerScope.Handler.HandleAsync(message, cancellationToken)
                            .ConfigureAwait(false);
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