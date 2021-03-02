using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves detailed information regarding a pending transaction by ID.
        /// </summary>
        /// <param name="pending">
        /// The identifier (Address/Schedule ID) of the pending transaction to retrieve.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A detailed description of the pending transaction, 
        /// including the serialized pending transaction body itself.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<PendingTransactionInfo> GetPendingTransactionInfo(Address pending, Action<IContext>? configure = null)
        {
            pending = RequireInputParameter.Pending(pending);
            await using var context = CreateChildContext(configure);
            var query = new Query
            {
                ScheduleGetInfo = new ScheduleGetInfoQuery
                {
                    ScheduleID = new ScheduleID(pending)
                }
            };
            var response = await query.SignAndExecuteWithRetryAsync(context);
            return response.ScheduleGetInfo.ScheduleInfo.ToPendingTransactionInfo();
        }
    }
}
