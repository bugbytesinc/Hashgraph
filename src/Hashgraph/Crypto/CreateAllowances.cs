using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <remarks>
    /// MARKED INTERNAL because this feature is not 
    /// implemented in full by the network and should
    /// not be made publicly available.
    /// </remarks>
    internal async Task<TransactionReceipt> CreateAllowancesAsync(AllowanceParams allowanceParams, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoApproveAllowanceTransactionBody(allowanceParams), configure, false, allowanceParams.Signatory).ConfigureAwait(false));
    }
    /// <remarks>
    /// MARKED INTERNAL because this feature is not 
    /// implemented in full by the network and should
    /// not be made publicly available.
    /// </remarks>
    internal async Task<AllowanceRecord> CreateAllowancesWithRecordAsync(AllowanceParams allowanceParams, Action<IContext>? configure = null)
    {
        return new AllowanceRecord(await ExecuteTransactionAsync(new CryptoApproveAllowanceTransactionBody(allowanceParams), configure, true, allowanceParams.Signatory).ConfigureAwait(false));
    }
}