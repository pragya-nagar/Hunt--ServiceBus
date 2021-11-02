using System;
using Synergy.ServiceBus.Abstracts;

namespace Synergy.ServiceBus.Amazon.Tests
{
    internal class FakeCommand_01 : ICommand
    {
        public Guid CorrelationId { get; set; }
        public DateTime CreatedOn { get; } = new DateTime(2000, 1, 1);
        public Guid Id { get; } = Guid.Parse("12345678-abcd-abcd-abcd-123456789012");
    }

    internal class FakeCommand_02 : ICommand
    {
        public Guid CorrelationId { get; set; }
        public DateTime CreatedOn { get; } = new DateTime(2000, 1, 1);
        public Guid Id { get; } = Guid.Parse("11111111-abcd-abcd-abcd-123456789012");
    }
}
