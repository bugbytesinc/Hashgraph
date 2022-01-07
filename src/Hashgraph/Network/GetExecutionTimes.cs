using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Retrieves the time in nanoseconds spent by the gossip node in 
    /// processing the identified transaction.  The storage of this 
    /// information varies by node and network configuration and may
    /// not be enabled on all networks.
    /// </summary>
    /// <param name="transactionIds">
    /// Array of Transaction IDs to query.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The executions time(s) in nano seconds for the requested transactions.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    public async Task<IReadOnlyCollection<ulong>> GetExecutionTimes(IEnumerable<TxId> transactionIds, Action<IContext>? configure = null)
    {
        return (await ExecuteQueryAsync(new NetworkGetExecutionTimeQuery(transactionIds), configure).ConfigureAwait(false)).NetworkGetExecutionTime.ExecutionTimes;
    }
}