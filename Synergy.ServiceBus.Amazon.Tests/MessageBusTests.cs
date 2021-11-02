using Amazon.SimpleNotificationService;
using Amazon.SimpleNotificationService.Model;
using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using Synergy.ServiceBus.Abstracts;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Amazon.Tests
{
    [TestFixture]
    public class MessageBusTests
    {
        [Test]
        public void MessageBus_Cunstruction()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();


            // Action
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Assert
            Assert.IsInstanceOf<ISubscribeMessage>(messageBus);
            Assert.IsInstanceOf<IPublishMessage>(messageBus);
            Assert.IsInstanceOf<IDisposable>(messageBus);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Subscribe_Null_Instead_Handler_Factory_Throw_ArgumentNullException()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await messageBus.SubscribeAsync<IMessage>(null));


            Assert.AreEqual("factory", ex.ParamName);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public void Publish_Null_Instead_Message_Instance_Throw_ArgumentNullException()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            var ex = Assert.ThrowsAsync<ArgumentNullException>(async () => await messageBus.PublishAsync((FakeCommand_01)null));


            // Assert
            Assert.AreEqual("message", ex.ParamName);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Publish_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILogger<MessageBus>>();
            var loggerFactoryMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();

            loggerFactoryMock.Setup(x => x.CreateLogger(It.IsAny<string>()))
                .Returns(() => loggerMock.Object);
            snsClientMock.Setup(x => x.CreateTopicAsync(It.IsAny<CreateTopicRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new CreateTopicResponse { HttpStatusCode = HttpStatusCode.OK, TopicArn = "test_topic_arn" });
            snsClientMock.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PublishResponse { HttpStatusCode = HttpStatusCode.OK });

            var messageBus = new MessageBus(loggerFactoryMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");

            var topicName = $"{typeof(FakeCommand_01).Name}_{typeof(FakeCommand_01).GUID}";

            // Action
            await messageBus.PublishAsync(new FakeCommand_01());


            // Assert
            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.PublishAsync(It.Is<PublishRequest>(y =>
                y.TopicArn == "test_topic_arn"
                && string.IsNullOrWhiteSpace(y.Message) == false
                && y.MessageAttributes.ContainsKey("ClrType")
                && y.MessageAttributes["ClrType"].DataType == "String"
                && y.MessageAttributes["ClrType"].StringValue == typeof(FakeCommand_01).AssemblyQualifiedName), It.IsAny<CancellationToken>()), Times.Once);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Publish_Two_Different_Messages()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            snsClientMock.Setup(x => x.CreateTopicAsync(It.IsAny<CreateTopicRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new CreateTopicResponse { HttpStatusCode = HttpStatusCode.OK, TopicArn = "test_topic_arn" });
            snsClientMock.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PublishResponse { HttpStatusCode = HttpStatusCode.OK });

            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");

            var topicName_01 = $"{typeof(FakeCommand_01).Name}_{typeof(FakeCommand_01).GUID}";
            var topicName_02 = $"{typeof(FakeCommand_02).Name}_{typeof(FakeCommand_02).GUID}";

            // Action
            await messageBus.PublishAsync(new FakeCommand_01());
            await messageBus.PublishAsync(new FakeCommand_02());


            // Assert
            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName_01), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.PublishAsync(It.Is<PublishRequest>(y =>
                y.TopicArn == "test_topic_arn"
                && string.IsNullOrWhiteSpace(y.Message) == false
                && y.MessageAttributes.ContainsKey("ClrType")
                && y.MessageAttributes["ClrType"].DataType == "String"
                && y.MessageAttributes["ClrType"].StringValue == typeof(FakeCommand_01).AssemblyQualifiedName), It.IsAny<CancellationToken>()), Times.Once);

            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName_02), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.PublishAsync(It.Is<PublishRequest>(y =>
                y.TopicArn == "test_topic_arn"
                && string.IsNullOrWhiteSpace(y.Message) == false
                && y.MessageAttributes.ContainsKey("ClrType")
                && y.MessageAttributes["ClrType"].DataType == "String"
                && y.MessageAttributes["ClrType"].StringValue == typeof(FakeCommand_02).AssemblyQualifiedName), It.IsAny<CancellationToken>()), Times.Once);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Publish_Two_Identical_Messages()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            snsClientMock.Setup(x => x.CreateTopicAsync(It.IsAny<CreateTopicRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new CreateTopicResponse { HttpStatusCode = HttpStatusCode.OK, TopicArn = "test_topic_arn" });
            snsClientMock.Setup(x => x.PublishAsync(It.IsAny<PublishRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => new PublishResponse { HttpStatusCode = HttpStatusCode.OK });

            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");

            var topicName = $"{typeof(FakeCommand_01).Name}_{typeof(FakeCommand_01).GUID}";

            // Action
            await messageBus.PublishAsync(new FakeCommand_01());
            await messageBus.PublishAsync(new FakeCommand_01());


            // Assert
            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.PublishAsync(It.Is<PublishRequest>(y =>
                y.TopicArn == "test_topic_arn"
                && string.IsNullOrWhiteSpace(y.Message) == false
                && y.MessageAttributes.ContainsKey("ClrType")
                && y.MessageAttributes["ClrType"].DataType == "String"
                && y.MessageAttributes["ClrType"].StringValue == typeof(FakeCommand_01).AssemblyQualifiedName), It.IsAny<CancellationToken>()), Times.Exactly(2));

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Subscribe_Message_Handler()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);


            // Assert
            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Subscribe_On_Two_Messages()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_02>>().Object);


            // Assert
            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task Subscribe_Two_Handlers_On_Same_Message()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);


            // Assert
            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task StartListerning_With_Subscriptions_And_Empty_Queue()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            sqsClientMock.Setup(x => x.CreateQueueAsync(It.IsAny<CreateQueueRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateQueueRequest req, CancellationToken token) => new CreateQueueResponse { HttpStatusCode = HttpStatusCode.OK, QueueUrl = $"{req.QueueName}_url" });
            sqsClientMock.Setup(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()))
                .Callback((ReceiveMessageRequest req, CancellationToken token) => Task.Delay(TimeSpan.FromSeconds(20), token).Wait(token));

            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            snsClientMock.Setup(x => x.CreateTopicAsync(It.IsAny<CreateTopicRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((CreateTopicRequest req, CancellationToken token) => new CreateTopicResponse { HttpStatusCode = HttpStatusCode.OK, TopicArn = $"{req.Name}_topic_arn" });

            var sqsClient = sqsClientMock.Object;
            var snsClient = snsClientMock.Object;

            var messageBus = new MessageBus(loggerMock.Object, sqsClient, snsClient, "test_queue", "test_topic");

            var topicName1 = $"{typeof(FakeCommand_01).Name}_{typeof(FakeCommand_01).GUID}";
            var topicName2 = $"{typeof(FakeCommand_02).Name}_{typeof(FakeCommand_02).GUID}";

            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_01>>().Object);
            await messageBus.SubscribeAsync(() => new Mock<IMessageHandler<FakeCommand_02>>().Object);


            // Action
            await messageBus.StartListeningAsync();
            await Task.Delay(2000);


            // Assert
            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName1), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.CreateTopicAsync(It.Is<CreateTopicRequest>(y => y.Name == topicName2), It.IsAny<CancellationToken>()), Times.Once);
            sqsClientMock.Verify(x => x.CreateQueueAsync(It.Is<CreateQueueRequest>(y => y.QueueName == "test_queue"), It.IsAny<CancellationToken>()), Times.Once);
            snsClientMock.Verify(x => x.SubscribeQueueAsync($"{topicName1}_topic_arn", sqsClient, "test_queue_url"), Times.Once);
            snsClientMock.Verify(x => x.SubscribeQueueAsync($"{topicName2}_topic_arn", sqsClient, "test_queue_url"), Times.Once);
            sqsClientMock.Verify(x => x.ReceiveMessageAsync(It.IsAny<ReceiveMessageRequest>(), It.IsAny<CancellationToken>()), Times.AtLeastOnce);

            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }

        [Test]
        public async Task StartListerning_Without_Subscriptions()
        {
            // Arrange
            var loggerMock = new Mock<ILoggerFactory>();
            var sqsClientMock = new Mock<IAmazonSQS>();
            var snsClientMock = new Mock<IAmazonSimpleNotificationService>();
            var messageBus = new MessageBus(loggerMock.Object, sqsClientMock.Object, snsClientMock.Object, "test_queue", "test_topic");


            // Action
            await messageBus.StartListeningAsync();


            // Assert
            sqsClientMock.VerifyNoOtherCalls();
            snsClientMock.VerifyNoOtherCalls();
        }
    }
}
