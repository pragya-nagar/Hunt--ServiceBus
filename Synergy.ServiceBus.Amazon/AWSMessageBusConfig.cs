using Amazon;

namespace Synergy.ServiceBus.Amazon
{
    public class AWSMessageBusConfig
    {
        public AWSMessageBusConfig()
        {
            this.MaxReceiveCount = 15;
        }

        public RegionEndpoint Region { get; set; }

        public string QueueName { get; set; }

        public string TopicName { get; set; }

        public int? ParallellBlockSize { get; set; }

        public int? ThrottledBlockSize { get; set; }

        public int MaxReceiveCount { get; set; }
    }
}