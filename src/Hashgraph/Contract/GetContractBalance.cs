using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Retrieves the balance in tinybars from the network for a given contract.
    /// </summary>
    /// <param name="contract">
    /// The hedera network contract address to retrieve the balance of.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The balance of the associated contract.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    public async Task<ulong> GetContractBalanceAsync(Address contract, Action<IContext>? configure = null)
    {
        return (await ExecuteQueryAsync(CryptoGetAccountBalanceQuery.ForContract(contract), configure).ConfigureAwait(false)).CryptogetAccountBalance.Balance;
    }
}