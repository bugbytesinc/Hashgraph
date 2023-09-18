using Google.Protobuf;
using Hashgraph.Implementation;
using Hashgraph.Test.Fixtures;
using Proto;
using Xunit;

namespace Hashgraph.Tests;

public class AssetInfoTests
{
    [Fact(DisplayName = "AssetInfo: Equivalent AssetInfos are considered Equal")]
    public void EquivalentAssetInfosAreConsideredEqual()
    {
        var nftInfoResponse = GenerateRandomNftInfoResponse();
        var assetInfo1 = new AssetInfo(nftInfoResponse);
        var assetInfo2 = new AssetInfo(nftInfoResponse);
        Assert.Equal(assetInfo1, assetInfo2);
        Assert.True(assetInfo1 == assetInfo2);
        Assert.False(assetInfo1 != assetInfo2);
        Assert.Equal(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }
    [Fact(DisplayName = "AssetInfo: Disimilar AssetInfos are not considered Equal")]
    public void DisimilarAssetInfosAreNotConsideredEqual()
    {
        var assetInfo1 = new AssetInfo(GenerateRandomNftInfoResponse());
        var assetInfo2 = new AssetInfo(GenerateRandomNftInfoResponse());

        Assert.NotEqual(assetInfo1, assetInfo2);
        Assert.False(assetInfo1 == assetInfo2);
        Assert.True(assetInfo1 != assetInfo2);
        Assert.NotEqual(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }
    [Fact(DisplayName = "AssetInfo: Different Assets Result in Not Equal Asset Infos")]
    public void DifferentAssetsResultInNotEqualAssetInfos()
    {
        var nftInfoResponse = GenerateRandomNftInfoResponse();
        var assetInfo1 = new AssetInfo(nftInfoResponse);
        nftInfoResponse.TokenGetNftInfo.Nft.NftID.SerialNumber += 1;
        var assetInfo2 = new AssetInfo(nftInfoResponse);
        Assert.NotEqual(assetInfo1, assetInfo2);
        Assert.False(assetInfo1 == assetInfo2);
        Assert.True(assetInfo1 != assetInfo2);
        Assert.NotEqual(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }
    [Fact(DisplayName = "AssetInfo: Different Owner Result in Not Equal Asset Infos")]
    public void DifferentOwnerResultInNotEqualAssetInfos()
    {
        var nftInfoResponse = GenerateRandomNftInfoResponse();
        var assetInfo1 = new AssetInfo(nftInfoResponse);
        nftInfoResponse.TokenGetNftInfo.Nft.AccountID.AccountNum += 1;
        var assetInfo2 = new AssetInfo(nftInfoResponse);
        Assert.NotEqual(assetInfo1, assetInfo2);
        Assert.False(assetInfo1 == assetInfo2);
        Assert.True(assetInfo1 != assetInfo2);
        Assert.NotEqual(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }
    [Fact(DisplayName = "AssetInfo: Different Created Time Result in Not Equal Asset Infos")]
    public void DifferentCreatedTimeResultInNotEqualAssetInfos()
    {
        var nftInfoResponse = GenerateRandomNftInfoResponse();
        var assetInfo1 = new AssetInfo(nftInfoResponse);
        nftInfoResponse.TokenGetNftInfo.Nft.CreationTime.Seconds += 1;
        var assetInfo2 = new AssetInfo(nftInfoResponse);
        Assert.NotEqual(assetInfo1, assetInfo2);
        Assert.False(assetInfo1 == assetInfo2);
        Assert.True(assetInfo1 != assetInfo2);
        Assert.NotEqual(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }
    [Fact(DisplayName = "AssetInfo: Different Metadata Result in Not Equal Asset Infos")]
    public void DifferentMetadataResultInNotEqualAssetInfos()
    {
        var nftInfoResponse = GenerateRandomNftInfoResponse();
        var assetInfo1 = new AssetInfo(nftInfoResponse);
        nftInfoResponse.TokenGetNftInfo.Nft.Metadata = ByteString.CopyFrom(Generator.SHA384Hash().Span);
        var assetInfo2 = new AssetInfo(nftInfoResponse);
        Assert.NotEqual(assetInfo1, assetInfo2);
        Assert.False(assetInfo1 == assetInfo2);
        Assert.True(assetInfo1 != assetInfo2);
        Assert.NotEqual(assetInfo1.GetHashCode(), assetInfo2.GetHashCode());
    }

    private static Response GenerateRandomNftInfoResponse()
    {
        var (seconds, nanos) = Epoch.UniqueSecondsAndNanos(false);
        var nftInfo = new TokenNftInfo
        {
            NftID = new NftID { TokenId = new TokenID { TokenNum = Generator.Integer(100, 200) }, SerialNumber = Generator.Integer(100, 200) },
            AccountID = new Proto.AccountID { AccountNum = Generator.Integer(100, 200) },
            CreationTime = new Timestamp { Seconds = seconds, Nanos = nanos },
            Metadata = ByteString.CopyFrom(Generator.SHA384Hash().Span)
        };
        return new Response { TokenGetNftInfo = new TokenGetNftInfoResponse { Nft = nftInfo } };
    }
}