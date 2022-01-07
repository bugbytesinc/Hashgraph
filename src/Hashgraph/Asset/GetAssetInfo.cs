using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph;

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
}