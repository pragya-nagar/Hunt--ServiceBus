using System;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public class FilterExecutionResult
    {
        private static readonly FilterExecutionResult Success = new FilterExecutionResult();

        private Exception _exception;

        private FilterExecutionResult()
        {
        }

        public bool SupressMessage { get; private set; }

        public Exception Exception
        {
            get
            {
                return this._exception;
            }

            private set
            {
                this._exception = value;

                if (this.Exception != null)
                {
                    this.SupressMessage = true;
                }
            }
        }

        public static FilterExecutionResult Sucess()
        {
            return Success;
        }

        public static FilterExecutionResult Error(Exception exception)
        {
            return new FilterExecutionResult()
            {
                Exception = exception,
            };
        }

        public static FilterExecutionResult Supress()
        {
            return new FilterExecutionResult()
            {
                SupressMessage = true,
            };
        }
    }
}