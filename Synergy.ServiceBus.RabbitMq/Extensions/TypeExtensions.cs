using System;
using System.Linq;

namespace Synergy.ServiceBus.RabbitMq.Extensions
{
    public static class TypeExtensions
    {
        public static string ToMessageType(this Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            return string.Join(", ", type.AssemblyQualifiedName.Split(',').Select(x => x.Trim()).Take(2));
        }
    }
}