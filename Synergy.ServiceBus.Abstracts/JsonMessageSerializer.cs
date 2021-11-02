using System;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Synergy.ServiceBus.Abstracts
{
    public class JsonMessageSerializer : IMessageSerializer
    {
        public Task<string> SerializeMessageAsync(IMessage source, CancellationToken cancellationToken)
        {
            var json = JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });

            return Task.FromResult(json);
        }

        public Task<object> DeserializeMessageAsync(string source, Type type, CancellationToken cancellationToken)
        {
            var result = JsonConvert.DeserializeObject(source, type, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            });

            return Task.FromResult(result);
        }
    }
}