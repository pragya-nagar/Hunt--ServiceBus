using System;
using System.Collections.Generic;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public class FilterExecutionContext
    {
        public FilterExecutionContext()
        {
            this.Metadata = new Dictionary<string, string>();
        }

        public FilterExecutionContext(Dictionary<string, string> metadata)
        {
            this.Metadata = metadata ?? throw new ArgumentNullException(nameof(metadata));
        }

        public Dictionary<string, string> Metadata { get; }
    }
}