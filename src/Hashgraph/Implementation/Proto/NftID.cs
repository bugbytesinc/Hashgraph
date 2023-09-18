using Hashgraph;
using System;

namespace Proto;

public sealed partial class NftID
{
    internal NftID(Asset asset) : this()
    {
        if (asset is null || asset == Asset.None)
        {
            throw new ArgumentNullException(nameof(asset), "Asset is missing. Please check that it is not null or empty.");
        }
        TokenId = new TokenID(asset);
        SerialNumber = asset.SerialNum;
    }
}
internal static class NftIDExtensions
{
    internal static Asset AsAsset(this NftID? id)
    {
        if (id is not null && id.TokenId is not null)
        {
            return new Asset(id.TokenId.ShardNum, id.TokenId.RealmNum, id.TokenId.TokenNum, id.SerialNumber);
        }
        return Asset.None;
    }
}