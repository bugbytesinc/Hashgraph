using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Creates approved allowance(s) alowing the designated
    /// agent to spend crypto and tokens from the originating
    /// account.  Presently the owning account must be the 
    /// Payer (operator) paying for this transaction when 
    /// submitted to the network.
    /// </summary>
    /// <param name="allowanceParams">
    /// Parameters containing the list of allowances to create.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction record indicating success, or an exception is thrown.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> CreateAllowancesAsync(AllowanceParams allowanceParams, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoApproveAllowanceTransactionBody(allowanceParams), configure, false, allowanceParams.Signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Creates approved allowance(s) alowing the designated
    /// agent to spend crypto and tokens from the originating
    /// account.  Presently the owning account must be the 
    /// Payer (operator) paying for this transaction when 
    /// submitted to the network.  Returns the record for the
    /// transaction.
    /// </summary>
    /// <param name="allowanceParams"></param>
    /// <param name="configure"></param>
    /// <param name="allowanceParams">
    /// Parameters containing the list of allowances to create.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A Transaction record indicating success, or an exception is thrown.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionRecord> CreateAllowancesWithRecordAsync(AllowanceParams allowanceParams, Action<IContext>? configure = null)
    {
        return new TransactionRecord(await ExecuteTransactionAsync(new CryptoApproveAllowanceTransactionBody(allowanceParams), configure, true, allowanceParams.Signatory).ConfigureAwait(false));
    }
}