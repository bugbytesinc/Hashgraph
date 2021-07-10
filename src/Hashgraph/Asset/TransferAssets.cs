using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Transfer assets (NFTs) from one account to another.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to transfer (shard, realm, num, serial).
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the assets from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// assets out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the assets.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> TransferAssetAsync(Asset asset, Address fromAddress, Address toAddress, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(asset, fromAddress, toAddress), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Transfer assets (NFTs) from one account to another.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to transfer (shard, realm, num, serial).
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the assets from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the assets.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer receipt indicating success of the operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionReceipt> TransferAssetAsync(Asset asset, Address fromAddress, Address toAddress, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionReceipt(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(asset, fromAddress, toAddress), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Transfer assets (NFTs) from one account to another.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to transfer (shard, realm, num, serial).
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the assets from.  Ensure that
        /// a signatory either in the context can fulfill the signing 
        /// requrements to transfer crypto out of the account identified 
        /// by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the assets.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> TransferAssetWithRecordAsync(Asset asset, Address fromAddress, Address toAddress, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(asset, fromAddress, toAddress), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Transfer assets (NFTs) from one account to another.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to transfer (shard, realm, num, serial).
        /// </param>
        /// <param name="fromAddress">
        /// The address to transfer the assets from.  Ensure that
        /// a signatory either in the context or passed with this
        /// call can fulfill the signing requrements to transfer 
        /// crypto out of the account identified by this address.
        /// </param>
        /// <param name="toAddress">
        /// The address receiving the assets.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the requirements for the sending and receiving accounts.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transfer record describing the details of the concensus transaction.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TransactionRecord> TransferAssetWithRecordAsync(Asset asset, Address fromAddress, Address toAddress, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TransactionRecord(await ExecuteTransactionAsync(new CryptoTransferTransactionBody(asset, fromAddress, toAddress), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
