using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Destroys an asset (NFT) instance.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to destory.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenReceipt> BurnAssetAsync(Asset asset, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(asset), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys multiple asset (NFT) instances.
        /// </summary>
        /// <param name="token">
        /// The identifier of the asset token type to destory.
        /// </param>
        /// <param name="serialNumbers">
        /// The serial numbers of the assets to destroy.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenReceipt> BurnAssetsAsync(Address token, IEnumerable<long> serialNumbers, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, serialNumbers), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys an asset (NFT) instance.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to destory.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this asset (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenReceipt> BurnAssetAsync(Asset asset, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(asset), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys multiple asset (NFT) instances.
        /// </summary>
        /// <param name="token">
        /// The identifier of the asset token type to destory.
        /// </param>
        /// <param name="serialNumbers">
        /// The serial numbers of the assets to destroy.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this asset (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction receipt indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenReceipt> BurnAssetsAsync(Address token, IEnumerable<long> serialNumbers, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, serialNumbers), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys an asset (NFT) instance.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to destory.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenRecord> BurnAssetWithRecordAsync(Asset asset, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(asset), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys multiple asset (NFT) instances.
        /// </summary>
        /// <param name="token">
        /// The identifier of the asset token type to destory.
        /// </param>
        /// <param name="serialNumbers">
        /// The serial numbers of the assets to destroy.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenRecord> BurnAssetsWithRecordAsync(Address token, IEnumerable<long> serialNumbers, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, serialNumbers), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys an asset (NFT) instance.
        /// </summary>
        /// <param name="asset">
        /// The identifier of the asset to destory.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this asset (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenRecord> BurnAssetWithRecordAsync(Asset asset, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(asset), configure, true, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Destroys multiple asset (NFT) instances.
        /// </summary>
        /// <param name="token">
        /// The identifier of the asset token type to destory.
        /// </param>
        /// <param name="serialNumbers">
        /// The serial numbers of the assets to destroy.
        /// </param>
        /// <param name="signatory">
        /// Additional signing key matching the administrative endorsements
        /// associated with this asset (if not already added in the context).
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A transaction record indicating a successful operation.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission, for example of the asset is already deleted.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<TokenRecord> BurnAssetsWithRecordAsync(Address token, IEnumerable<long> serialNumbers, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenBurnTransactionBody(token, serialNumbers), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
