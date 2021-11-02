using System;
using System.Threading;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts
{
    public interface IMessageSerializer
    {
        Task<string> SerializeMessageAsync(IMessage source, CancellationToken cancellationToken);

        Task<object> DeserializeMessageAsync(string source, Type type, CancellationToken cancellationToken);
    }
}
