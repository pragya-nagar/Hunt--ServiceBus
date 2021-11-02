using System;

using Newtonsoft.Json;

namespace Synergy.ServiceBus.Abstracts
{
    public abstract class Command : ICommand
    {
        [JsonProperty]
        public Guid Id { get; protected set; }

        [JsonProperty]
        public DateTime CreatedOn { get; protected set; }

        [JsonProperty]
        public Guid CreatedBy { get; protected set; }

        public static T Create<T>(Guid id, Guid userId)
            where T : Command, new()
        {
            if (id == Guid.Empty)
            {
                throw new ArgumentException("Message id can not be empty", nameof(id));
            }

            if (userId == Guid.Empty)
            {
                throw new ArgumentException("Message userId can not be empty", nameof(userId));
            }

            return new T
            {
                Id = id,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = userId,
            };
        }
    }
}
