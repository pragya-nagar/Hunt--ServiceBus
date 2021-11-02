namespace Synergy.ServiceBus.Extensions.Progress
{
    public interface IProgressScopeFactory
    {
        void Create(IProgressScope scope);
    }
}