using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Removes the holdings of given asset from the associated 
        /// account and destorys them. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="asset">
        /// The identifier (Address &amp; Serial Number) of the asset to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the asset to remove.
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
        public async Task<TokenReceipt> ConfiscateAssetAsync(Asset asset, AddressOrAlias address, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(asset, address), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of multiple assets from the associated 
        /// account and destorys them. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// Address of the asset type.
        /// </param>
        /// <param name="serialNumbers">
        /// Serial Numbers of assets to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the assets to remove.
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
        public async Task<TokenReceipt> ConfiscateAssetsAsync(Address token, IEnumerable<long> serialNumbers, AddressOrAlias address, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(token, serialNumbers, address), configure, false).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of given asset from the associated 
        /// account and destorys them. Must be signed by the 
        /// confiscate/wipe admin key.
        /// </summary>
        /// <param name="asset">
        /// The identifier (Address &amp; Serial Number) of the asset to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the asset to remove.
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
        public async Task<TokenReceipt> ConfiscateAssetAsync(Asset asset, AddressOrAlias address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(asset, address), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of multiple assets from the associated 
        /// account and destorys them. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// Address of the asset type.
        /// </param>
        /// <param name="serialNumbers">
        /// Serial Numbers of assets to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the assets to remove.
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
        public async Task<TokenReceipt> ConfiscateAssetsAsync(Address token, IEnumerable<long> serialNumbers, AddressOrAlias address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenReceipt(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(token, serialNumbers, address), configure, false, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of given asset from the associated 
        /// account and destorys them. Must be signed by the 
        /// confiscate/wipe admin key.
        /// </summary>
        /// <param name="asset">
        /// The identifier (Address &amp; Serial Number) of the asset to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the asset to remove.
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
        public async Task<TokenRecord> ConfiscateAssetWithRecordAsync(Asset asset, AddressOrAlias address, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(asset, address), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of multiple assets from the associated 
        /// account and destorys them. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// Address of the asset type.
        /// </param>
        /// <param name="serialNumbers">
        /// Serial Numbers of assets to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the assets to remove.
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
        public async Task<TokenRecord> ConfiscateAssetsWithRecordAsync(Address token, IEnumerable<long> serialNumbers, AddressOrAlias address, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(token, serialNumbers, address), configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of given asset from the associated 
        /// account and destorys them. Must be signed by the 
        /// confiscate/wipe admin key.
        /// </summary>
        /// <param name="asset">
        /// The identifier (Address &amp; Serial Number) of the asset to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the asset to remove.
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
        public async Task<TokenRecord> ConfiscateAssetWithRecordAsync(Asset asset, AddressOrAlias address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(asset, address), configure, true, signatory).ConfigureAwait(false));
        }
        /// <summary>
        /// Removes the holdings of multiple assets from the associated 
        /// account and destorys them. Must be signed by 
        /// the confiscate/wipe admin key.
        /// </summary>
        /// <param name="token">
        /// Address of the asset type.
        /// </param>
        /// <param name="serialNumbers">
        /// Serial Numbers of assets to confiscate.
        /// </param>
        /// <param name="address">
        /// Address of the account holding the assets to remove.
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
        public async Task<TokenRecord> ConfiscateAssetsWithRecordAsync(Address token, IEnumerable<long> serialNumbers, Address address, Signatory signatory, Action<IContext>? configure = null)
        {
            return new TokenRecord(await ExecuteTransactionAsync(new TokenWipeAccountTransactionBody(token, serialNumbers, address), configure, true, signatory).ConfigureAwait(false));
        }
    }
}
