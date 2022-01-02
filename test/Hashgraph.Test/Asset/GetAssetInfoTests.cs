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
            // NETWORK V0.21.0 DEFECT vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 DEFECT: ^^^^
        }
    }
}
