using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public class DefaultFilterProvider : IFilterProvider
    {
        private readonly IEnumerable<IMessageFilter> _filters;

        private readonly ConcurrentDictionary<Type, IEnumerable<IMessageFilter>> _cache = new ConcurrentDictionary<Type, IEnumerable<IMessageFilter>>();

        public DefaultFilterProvider(IEnumerable<IMessageFilter> filters)
        {
            this._filters = filters;
        }

        public IEnumerable<IMessageFilter> GetFilters(IMessage msg)
        {
            if (this._filters.Any() == false)
            {
                return Enumerable.Empty<IMessageFilter>();
            }

            return this._cache.GetOrAdd(msg.GetType(), typeToFilter => this._filters.Where(f => this.CanFilterMessage(typeToFilter, f)));
        }

        private bool CanFilterMessage(Type msgType, IMessageFilter filter)
        {
            var filterType = filter.GetType();

            var openGenericFilterType = typeof(IMessageFilter<>);

            var isGenericFilter = filterType.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == openGenericFilterType);

            // Non typed filter can filter any massage
            if (isGenericFilter == false)
            {
                return true;
            }

            // True is generic filter is compatible with current msg type
            return openGenericFilterType.MakeGenericType(msgType).IsAssignableFrom(filterType);
        }
    }
}