using System.Collections.Generic;
using System.Linq;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public class EmptyFilterProvider : IFilterProvider
    {
        public IEnumerable<IMessageFilter> GetFilters(IMessage msg)
        {
            return Enumerable.Empty<IMessageFilter>();
        }
    }
}