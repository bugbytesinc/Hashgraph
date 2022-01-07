using Proto;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Hashgraph;

public partial class Client
{
    /// <summary>
    /// Adds asset coins to the treasury.
    /// </summary>
    /// <param name="asset">
    /// The identifier (Address/Symbol) of the asset to add coins to.
    /// </param>
    /// <param name="metadata">
    /// A list of metadata, a series of bytes (not to exceed 100), one
    /// for each asset that is created.
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
    public async Task<AssetMintReceipt> MintAssetAsync(Address asset, IEnumerable<ReadOnlyMemory<byte>> metadata, Action<IContext>? configure = null)
    {
        return new AssetMintReceipt(await ExecuteTransactionAsync(new TokenMintTransactionBody(asset, metadata), configure, false).ConfigureAwait(false));
    }
    /// <summary>
    /// Adds asset coins to the treasury.
    /// </summary>
    /// <param name="asset">
    /// The identifier (Address/Symbol) of the asset to add coins to.
    /// </param>
    /// <param name="metadata">
    /// A list of metadata, a series of bytes (not to exceed 100), one
    /// for each asset that is created.
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
    public async Task<AssetMintReceipt> MintAssetAsync(Address asset, IEnumerable<ReadOnlyMemory<byte>> metadata, Signatory signatory, Action<IContext>? configure = null)
    {
        return new AssetMintReceipt(await ExecuteTransactionAsync(new TokenMintTransactionBody(asset, metadata), configure, false, signatory).ConfigureAwait(false));
    }
    /// <summary>
    /// Adds asset coins to the treasury.
    /// </summary>
    /// <param name="asset">
    /// The identifier (Address/Symbol) of the asset to add coins to.
    /// </param>
    /// <param name="metadata">
    /// A list of metadata, a series of bytes (not to exceed 100), one
    /// for each asset that is created.
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
    public async Task<AssetMintRecord> MintAssetWithRecordAsync(Address asset, IEnumerable<ReadOnlyMemory<byte>> metadata, Action<IContext>? configure = null)
    {
        return new AssetMintRecord(await ExecuteTransactionAsync(new TokenMintTransactionBody(asset, metadata), configure, true).ConfigureAwait(false));
    }
    /// <summary>
    /// Adds asset coins to the treasury.
    /// </summary>
    /// <param name="asset">
    /// The identifier (Address/Symbol) of the asset to add coins to.
    /// </param>
    /// <param name="metadata">
    /// A list of metadata, a series of bytes (not to exceed 100), one
    /// for each asset that is created.
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
    public async Task<AssetMintRecord> MintAssetWithRecordAsync(Address asset, IEnumerable<ReadOnlyMemory<byte>> metadata, Signatory signatory, Action<IContext>? configure = null)
    {
        return new AssetMintRecord(await ExecuteTransactionAsync(new TokenMintTransactionBody(asset, metadata), configure, true, signatory).ConfigureAwait(false));
    }
}