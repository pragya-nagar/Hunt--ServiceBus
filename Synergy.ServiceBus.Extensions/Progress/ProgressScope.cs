using System;

namespace Synergy.ServiceBus.Extensions.Progress
{
    public class ProgressScope : IProgressScope
    {
        private double _currentProgress;

        public ProgressScope(Guid userId, bool isTerminal = true, int scopeSize = 100)
        {
            if (userId == Guid.Empty)
            {
                throw new ArgumentException("userId should not be empty", nameof(userId));
            }

            if (scopeSize > 100 || scopeSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scopeSize), "Progress scope size should be in range 1..100");
            }

            this.ScopeSize = scopeSize;
            this.UserId = userId;
            this.IsTerminal = isTerminal;
        }

        public ProgressScope(IProgressScope parent, int scopeSize = 100)
        {
            if (scopeSize > 100 || scopeSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(scopeSize), "Progress scope size should be in range 1..100");
            }

            this.Parent = parent ?? throw new ArgumentNullException(nameof(parent));
            this.UserId = parent.UserId;
            this.IsTerminal = parent.IsTerminal;
            this.ScopeSize = scopeSize;
        }

        public Guid UserId { get; }

        public double CurrentProgress
        {
            get => this._currentProgress;
            set => this._currentProgress = value > 100 ? 100 : value;
        }

        public IProgressScope Parent { get; }

        public bool IsTerminal { get; }

        public double TotalProgress => (this.Parent?.TotalProgress ?? 0.0) + (this.AbsoluteSize * (this.CurrentProgress / 100.0));

        internal int ScopeSize { get; }

        internal int AbsoluteSize => (int)((this.ScopeSize / 100.0) * (this.Parent as ProgressScope)?.AbsoluteSize ?? 100);
    }
}