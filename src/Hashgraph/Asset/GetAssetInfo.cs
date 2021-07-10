using Proto;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Retrieves detailed information regarding a particular asset (NFT) instance.
        /// </summary>
        /// <param name="asset">
        /// The identifier (Address and Serial Number) of the asset.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A description of the asset instance, including metadata, created date and current owning account.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<AssetInfo> GetAssetInfoAsync(Asset asset, Action<IContext>? configure = null)
        {
            return new AssetInfo(await ExecuteQueryAsync(new TokenGetNftInfoQuery(asset), configure).ConfigureAwait(false));
        }
        /// <summary>
        /// Retrieves a list detailing information regarding a particular serial
        /// number range of a particular asset (NFT) class.
        /// </summary>
        /// <param name="token">
        /// The asset token address for this asset class.
        /// </param>
        /// <param name="startingIndex">
        /// The starting index of the range of assets to retrieve.
        /// </param>
        /// <param name="count">
        /// The number of assets to return in this method call.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A list of token asset instances indicating serial number, metadata
        /// and ownership in sequence.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<ReadOnlyCollection<AssetInfo>> GetAssetInfoAsync(Address token, long startingIndex, long count, Action<IContext>? configure = null)
        {
            return AssetInfo.AssetInfoCollection(await ExecuteQueryAsync(new TokenGetNftInfosQuery(token, startingIndex, count), configure).ConfigureAwait(false));
        }
        /// <summary>
        /// Retrieves a list detailing the assets (NFT) held by a
        /// particular account.
        /// </summary>
        /// <param name="account">
        /// The address of the acount holding the assets.
        /// </param>
        /// <param name="startingIndex">
        /// The starting index of the range of assets to retrieve.
        /// </param>
        /// <param name="count">
        /// The number of assets to return in this method call.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A list of token asset instances indicating serial number, metadata
        /// and ownership in sequence.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        public async Task<ReadOnlyCollection<AssetInfo>> GetAccountAssetInfoAsync(Address account, long startingIndex, long count, Action<IContext>? configure = null)
        {
            return AssetInfo.AssetInfoCollection(await ExecuteQueryAsync(new TokenGetAccountNftInfosQuery(account, startingIndex, count), configure).ConfigureAwait(false));
        }
    }
}
