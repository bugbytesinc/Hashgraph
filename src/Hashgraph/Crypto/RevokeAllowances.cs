using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Removes approved spending allowance(s) for specific assets (NFTs).
    /// </summary>
    /// <param name="token">
    /// The token id of the asset.
    /// </param>
    /// <param name="owner">
    /// The current asset owner.
    /// </param>
    /// <param name="serialNumbers">
    /// One or more serial numbers of the assets that have previously
    /// been added to the spending allowance list for a given spender.
    /// </param>
    /// <param name="signatory">
    /// The signatory containing any additional private keys or callbacks
    /// to meet the key signing requirements for participants.
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
    public async Task<TransactionReceipt> RevokeAssetAllowancesAsync(Address token, Address owner, IReadOnlyCollection<long> serialNumbers, Signatory? signatory = null, Action<IContext>? configure = null)
    {
        return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoDeleteAllowanceTransactionBody(token,owner,serialNumbers), configure, false, signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Removes approved spending allowance(s) for specific assets (NFTs).
    /// Returns the record for the transaction.
    /// </summary>
    /// <param name="token">
    /// The token id of the asset.
    /// </param>
    /// <param name="owner">
    /// The current asset owner.
    /// </param>
    /// <param name="serialNumbers">
    /// One or more serial numbers of the assets that have previously
    /// been added to the spending allowance list for a given spender.
    /// </param>
    /// <param name="signatory">
    /// The signatory containing any additional private keys or callbacks
    /// to meet the key signing requirements for participants.
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
    public async Task<TransactionRecord> RevokeAssetAllowancesWithRecordAsync(Address token, Address owner, IReadOnlyCollection<long> serialNumbers, Signatory? signatory = null, Action<IContext>? configure = null)
    {
        return new TransactionRecord(await ExecuteTransactionAsync(new CryptoDeleteAllowanceTransactionBody(token, owner, serialNumbers), configure, true, signatory).ConfigureAwait(false));
    }
}