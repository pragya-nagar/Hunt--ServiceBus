using System;

namespace Synergy.ServiceBus.Extensions.Progress
{
    public interface IProgressScope
    {
        IProgressScope Parent { get; }

        double CurrentProgress { get; set; }

        double TotalProgress { get; }

        bool IsTerminal { get; }

        Guid UserId { get; }
    }
}