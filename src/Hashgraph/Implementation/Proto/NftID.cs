using Hashgraph;
using System;

namespace Proto
{
    public sealed partial class NftID
    {
        internal NftID(Asset asset) : this()
        {
            if (asset is null || asset == Asset.None)
            {
                throw new ArgumentNullException(nameof(asset), "Asset is missing. Please check that it is not null or empty.");
            }
            TokenID = new TokenID(asset);
            SerialNumber = asset.SerialNum;
        }
    }
    internal static class NftIDExtensions
    {
        internal static Asset AsAsset(this NftID? id)
        {
            if (id is not null && id.TokenID is not null)
            {
                return new Asset(id.TokenID.ShardNum, id.TokenID.RealmNum, id.TokenID.TokenNum, id.SerialNumber);
            }
            return Asset.None;
        }
    }

}
