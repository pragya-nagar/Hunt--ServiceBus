using System.Net;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Messages
{
    public class OperationStatusEvent : Event
    {
        public HttpStatusCode Code { get; set; }

        public int? Progress { get; set; }

        public string Message { get; set; }
    }
}
