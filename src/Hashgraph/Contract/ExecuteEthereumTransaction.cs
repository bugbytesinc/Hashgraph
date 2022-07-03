using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Submits an equivalent Ethereum transaction (native RLP encoded type 0, 1, and 2)
    /// transaction to the hedera network.
    /// </summary>
    /// <param name="transactionParams">The ethereum formatted transaction details.</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction receipt indicating success, it does not
    /// include any output parameters sent from the contract.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<TransactionReceipt> ExecuteEthereumTransactionAsync(EthereumTransactionParams transactionParams, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new EthereumTransactionBody(transactionParams), configure, false, transactionParams.Signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Submits an equivalent Ethereum transaction (native RLP encoded type 0, 1, and 2)
    /// transaction to the hedera network and then fetches the record for the receipt.
    /// </summary>
    /// <param name="transactionParams">The ethereum formatted transaction details.</param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify 
    /// the execution configuration for just this method call. 
    /// It is executed prior to submitting the request to the network.
    /// </param>
    /// <returns>
    /// A transaction record indicating success, it does not
    /// include any output parameters sent from the contract.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
    public async Task<EthereumRecord> ExecuteEthereumTransactionWithRecordAsync(EthereumTransactionParams transactionParams, Action<IContext>? configure = null)
    {
        return new EthereumRecord(await ExecuteTransactionAsync(new EthereumTransactionBody(transactionParams), configure, true, transactionParams.Signatory).ConfigureAwait(false));
    }
}