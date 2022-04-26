using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Retrieves all details regarding a Hedera Network Account.
    /// </summary>
    /// <param name="address">
    /// The Hedera Network Address to retrieve details of.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A detailed description of the account.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    public async Task<AccountDetail> GetAccountDetailAsync(Address address, Action<IContext>? configure = null)
    {
        return new AccountDetail(await ExecuteQueryAsync(new GetAccountDetailsQuery(address), configure).ConfigureAwait(false));
    }
}