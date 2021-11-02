using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Synergy.ServiceBus.Abstracts.Filtering
{
    public static class Extensions
    {
        public static async Task ApplyHandleProcessingAsync(this IEnumerable<IMessageFilter> filters,
            IMessage message,
            HandleOptions handleOptions,
            FilterExecutionContext context,
            Func<Task> handlingBlock)
        {
            foreach (var filter in filters)
            {
                await filter.PreHandleMessageAsync(message, handleOptions, context).ConfigureAwait(false);
            }

            await handlingBlock().ConfigureAwait(false);
        }

        public static async Task ApplyReceiveProcessingAsync(this IEnumerable<IMessageFilter> filters,
                                                            IMessage message,
                                                            FilterExecutionContext context,
                                                            Func<Task> handlingBlock)
        {
            var filtersArray = filters.ToArray();

            for (var i = 0; i < filtersArray.Length; i++)
            {
                var filter = filtersArray[i];
                var res = await filter.ReceiveMessageAsync(message, context).ConfigureAwait(false);
                if (res.SupressMessage)
                {
                    if (res.Exception != null)
                    {
                        throw res.Exception;
                    }

                    return;
                }
            }

            await handlingBlock().ConfigureAwait(false);

            for (int i = filtersArray.Length - 1; i >= 0; i--)
            {
                var filter = filtersArray[i];

                await filter.PostHandleMessageAsync(message, context).ConfigureAwait(false);
            }
        }

        public static async Task ApplySendProcessingAsync(this IEnumerable<IMessageFilter> filters,
                                                          IMessage message,
                                                          FilterExecutionContext executionContext,
                                                          Func<Task> sendBlock)
        {
            foreach (var filter in filters)
            {
                var res = await filter.PreSendMessageAsync(message, executionContext).ConfigureAwait(false);
                if (res.SupressMessage)
                {
                    if (res.Exception != null)
                    {
                        throw res.Exception;
                    }

                    return;
                }
            }

            await sendBlock().ConfigureAwait(false);
        }
    }
}
