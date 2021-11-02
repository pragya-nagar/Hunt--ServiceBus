using System;
using System.Threading;

namespace Synergy.ServiceBus.Extensions.Progress
{
    public sealed class ProgressScopeStorage : IProgressScopeAccessor, IProgressScopeFactory, IDisposable
    {
        private static AsyncLocal<IProgressScope> _internalScope = new AsyncLocal<IProgressScope>();

        public IProgressScope Current => _internalScope.Value;

        public void Create(IProgressScope scope)
        {
            _internalScope.Value = scope;
        }

        public void Dispose()
        {
            _internalScope.Value = null;
        }
    }
}