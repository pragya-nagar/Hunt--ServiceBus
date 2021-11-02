using System.Collections.Generic;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public interface IFilterProvider
    {
        IEnumerable<IMessageFilter> GetFilters(IMessage msg);
    }
}
