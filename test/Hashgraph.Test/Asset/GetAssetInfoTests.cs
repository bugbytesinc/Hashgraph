using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens
{
    [Collection(nameof(NetworkCredentials))]
    public class GetAssetInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetAssetInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Asset Info: Can Get Asset Info")]
        public async Task CanGetAssetInfo()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            var asset = new Asset(fxAsset.Record.Token, 1);
            var receipt = await fxAsset.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount.Record.Address, fxAccount.Record.Address, fxAsset.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetAssetInfoAsync(asset);
            Assert.Equal(asset, info.Asset);
            Assert.Equal(fxAccount.Record.Address, info.Owner);
            Assert.Equal(fxAsset.MintRecord.Concensus, info.Created);
            Assert.Equal(fxAsset.Metadata[0].ToArray(), info.Metadata.ToArray());
        }
        [Fact(DisplayName = "NETWORK BUG: Asset Info: Can Get Multiple Asset Infos FAILS")]
        public async Task CanGetMultipleAssetInfosFails()
        {
            // tokenGetNftInfos is just plain 'ol borken now.
            var testFailException = (await Assert.ThrowsAsync<Hashgraph.PrecheckException>(CanGetMultipleAssetInfos));
            Assert.Equal(ResponseCode.FailInvalid, testFailException.Status);

            //[Fact(DisplayName = "Asset Info: Can Get Multiple Asset Infos")]
            async Task CanGetMultipleAssetInfos()
            {
                await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

                var list = await fxAsset.Client.GetAssetInfoAsync(fxAsset.Record.Token, 0, fxAsset.Metadata.Length);
                Assert.Equal(fxAsset.Metadata.Length, list.Count);

                for (var sn = 1; sn <= fxAsset.Metadata.Length; sn++)
                {
                    var metadata = fxAsset.Metadata[sn - 1];
                    var info = list.FirstOrDefault(i => i.Metadata.ToArray().SequenceEqual(metadata.ToArray()));
                    Assert.NotNull(info);
                    Assert.Equal(fxAsset.Record.Token, info.Asset);
                    Assert.Equal((long)sn, info.Asset.SerialNum);
                    Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Owner);
                    Assert.Equal(fxAsset.MintRecord.Concensus, info.Created);
                }
            }
        }
        [Fact(DisplayName = "NETWORK BUG: Asset Info: Can Get Multiple Account Asset Infos FAILS")]
        public async Task CanGetMultipleAccountAssetInfosFails()
        {
            // tokenGetAccountNftInfos is just plain 'ol borken now.
            var testFailException = (await Assert.ThrowsAsync<Xunit.Sdk.EqualException>(CanGetMultipleAccountAssetInfos));
            Assert.StartsWith("Assert.Equal() Failure", testFailException.Message);
            Assert.Equal("0", testFailException.Actual);

            //[Fact(DisplayName = "Asset Info: Can Get Multiple Account Asset Infos")]
            async Task CanGetMultipleAccountAssetInfos()
            {
                await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

                var list = await fxAsset.Client.GetAccountAssetInfoAsync(fxAsset.TreasuryAccount.Record.Address, 0, fxAsset.Metadata.Length);
                Assert.Equal(fxAsset.Metadata.Length, list.Count);

                for (var sn = 1; sn <= fxAsset.Metadata.Length; sn++)
                {
                    var metadata = fxAsset.Metadata[sn - 1];
                    var info = list.FirstOrDefault(i => i.Metadata.ToArray().SequenceEqual(metadata.ToArray()));
                    Assert.NotNull(info);
                    Assert.Equal(fxAsset.Record.Token, info.Asset);
                    Assert.Equal((long)sn, info.Asset.SerialNum);
                    Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Owner);
                    Assert.Equal(fxAsset.MintRecord.Concensus, info.Created);
                }
            }
        }
    }
}
