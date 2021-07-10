using Proto;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetTokenInfo Client 
    /// method call.  It represents the details concerning a 
    /// Hedera Fungable Token.
    /// </summary>
    public sealed record AssetInfo
    {
        /// <summary>
        /// The identifer of the asset (NFT Instance).
        /// </summary>
        public Asset Asset { get; private init; }
        /// <summary>
        /// The account currently owning the asset.
        /// </summary>
        public Address Owner { get; private init; }
        /// <summary>
        /// The Consensus Timestamp for when this asset was created (minted).
        /// </summary>
        public DateTime Created { get; private init; }
        /// <summary>
        /// The metadata associated with this asset, limited to 100 bytes.
        /// </summary>
        public ReadOnlyMemory<byte> Metadata { get; private init; }
        private AssetInfo(TokenNftInfo info)
        {
            Asset = info.NftID.AsAsset();
            Owner = info.AccountID.AsAddress();
            Created = info.CreationTime.ToDateTime();
            Metadata = info.Metadata.ToByteArray();
        }
        internal AssetInfo(Response response) : this(response.TokenGetNftInfo.Nft)
        {
        }
        public static ReadOnlyCollection<AssetInfo> AssetInfoCollection(Response response)
        {
            return response.ResponseCase == Response.ResponseOneofCase.TokenGetNftInfos ?
                response.TokenGetNftInfos.Nfts.Select(info => new AssetInfo(info)).ToList().AsReadOnly() :
                response.TokenGetAccountNftInfos.Nfts.Select(info => new AssetInfo(info)).ToList().AsReadOnly();
        }
    }
}
