using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Retrieves the crypto and token blances from the network for a given address.
    /// </summary>
    /// <param name="address">
    /// The hedera network address to retrieve the balance of.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// An object containing the crypto balance associated with the
    /// account in addition to a list of all tokens held by the account
    /// with their balances.
    /// </returns>
    public async Task<AccountBalances> GetAccountBalancesAsync(AddressOrAlias address, Action<IContext>? configure = null)
    {
        return new AccountBalances(await ExecuteQueryAsync(CryptoGetAccountBalanceQuery.ForAccount(address), configure).ConfigureAwait(false));
    }
    /// <summary>
    /// Retrieves the balance in tinybars from the network for a given address.
    /// </summary>
    /// <param name="address">
    /// The hedera network address to retrieve the balance of.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// The balance of the associated address.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    public async Task<ulong> GetAccountBalanceAsync(AddressOrAlias address, Action<IContext>? configure = null)
    {
        return new AccountBalances(await ExecuteQueryAsync(CryptoGetAccountBalanceQuery.ForAccount(address), configure).ConfigureAwait(false)).Crypto;
    }
}