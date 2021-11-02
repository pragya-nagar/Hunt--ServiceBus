using System;
using System.Text;

using Newtonsoft.Json;

using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.RabbitMq.Extensions
{
    public static class MessageExtensions
    {
        public static byte[] SerializeMessage(this IMessage source)
        {
            var json = JsonConvert.SerializeObject(source, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
            });

            return Encoding.UTF8.GetBytes(json);
        }

        public static object DeserializeMessage(this byte[] source, Type type)
        {
            var json = Encoding.UTF8.GetString(source);
            return json.DeserializeMessage(type);
        }

        public static object DeserializeMessage(this string source, Type type)
        {
            return JsonConvert.DeserializeObject(source, type, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore,
                ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor,
            });
        }
    }
}
