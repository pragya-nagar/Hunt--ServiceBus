namespace Synergy.ServiceBus.Extensions.Progress
{
    public interface IProgressScopeAccessor
    {
        IProgressScope Current { get; }
    }
}