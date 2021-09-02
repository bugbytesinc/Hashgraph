using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken
{
    [Collection(nameof(NetworkCredentials))]
    public class GetAssetTokenInfoTests
    {
        private readonly NetworkCredentials _network;
        public GetAssetTokenInfoTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Asset Token Info: Can Get Asset Token Info")]
        public async Task CanGetAssetTokenInfo()
        {
            await using var fx = await TestAsset.CreateAsync(_network);
            Assert.Equal(ResponseCode.Success, fx.Record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Equal(fx.Record.Token, info.Token);
            Assert.Equal(fx.Params.Symbol, info.Symbol);
            Assert.Equal(fx.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fx.Metadata.Length, info.Circulation);
            Assert.Equal(0u, info.Decimals);
            Assert.Equal(fx.Params.Ceiling, info.Ceiling);
            Assert.Equal(fx.Params.Administrator, info.Administrator);
            Assert.Equal(fx.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fx.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fx.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fx.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fx.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Commissions);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Asset Token Info: Support For Defect 2088 Supply Key Not Being Recorded (Reproduce Non Reproduce)")]
        public async Task SupportForDefect2088SupplyKeyNotBeingRecordedReproduceNonReproduce()
        {
            await using var fxTreasury = await TestAccount.CreateAsync(_network);
            var (adminPublicKey, adminPrivateKey) = Generator.KeyPair();
            var (supplyPublicKey, supplyPrivateKey) = Generator.KeyPair();
            var (commissionPublicKey, commissionPrivateKey) = Generator.KeyPair();
            var createParams = new CreateAssetParams
            {
                Name = "012345678912",
                Symbol = "ABCD",
                Treasury = fxTreasury,
                Administrator = adminPublicKey,
                SupplyEndorsement = supplyPublicKey,
                CommissionsEndorsement = commissionPublicKey,
                Expiration = DateTime.UtcNow.AddSeconds(7890000),
                Signatory = new Signatory(fxTreasury, adminPrivateKey)
            };
            var receipt = await fxTreasury.Client.CreateTokenAsync(createParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxTreasury.Client.GetTokenInfoAsync(receipt.Token);
            Assert.Equal(createParams.CommissionsEndorsement, info.CommissionsEndorsement);
        }
        [Fact(DisplayName = "Asset Token Info: Support For Defect 2088 Supply Key Not Being Recorded (Demonstrate Problem)")]
        public async Task SupportForDefect2088SupplyKeyNotBeingRecordedDemonstrateProblem()
        {
            await using var fxTreasury = await TestAccount.CreateAsync(_network);
            var (adminPublicKey, adminPrivateKey) = Generator.KeyPair();
            var (supplyPublicKey, supplyPrivateKey) = Generator.KeyPair();
            var (commissionPublicKey, commissionPrivateKey) = Generator.KeyPair();
            var createParams = new CreateAssetParams
            {
                Name = "012345678912",
                Symbol = "ABCD",
                Treasury = fxTreasury,
                Administrator = adminPublicKey,
                SupplyEndorsement = supplyPublicKey,
                CommissionsEndorsement = commissionPublicKey,
                Expiration = DateTime.UtcNow.AddSeconds(7890000),
                Signatory = new Signatory(fxTreasury, adminPrivateKey)
            };
            var receipt = await fxTreasury.Client.CreateTokenAsync(createParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var metadata = Enumerable.Range(1, 10).Select(_ => Generator.SHA384Hash()).ToArray();
            var record = await fxTreasury.Client.MintAssetWithRecordAsync(receipt.Token, metadata, supplyPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fxTreasury.Client.GetTokenInfoAsync(receipt.Token);
            Assert.Equal(createParams.CommissionsEndorsement, info.CommissionsEndorsement);
        }

        [Fact(DisplayName = "Asset Token Info: Null Asset Identifier Raises Exception")]
        public async Task NullTokenIdentifierRaisesException()
        {
            await using var fx = await TestAsset.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(null);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
        }
        [Fact(DisplayName = "Asset Token Info: Empty Address Identifier Raises Exception")]
        public async Task EmptyAddressIdentifierRaisesException()
        {
            await using var fx = await TestAsset.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(Address.None);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
        }
        [Fact(DisplayName = "Asset Token Info: Account Address for Asset Symbol Raises Error")]
        public async Task AccountAddressForTokenSymbolRaisesError()
        {
            await using var fx = await TestAccount.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(fx.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
        }
        [Fact(DisplayName = "Asset Token Info: Contract Address for Asset Symbol Raises Error")]
        public async Task ContractAddressForTokenSymbolRaisesError()
        {
            await using var fx = await GreetingContract.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(fx.ContractRecord.Contract);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
        }
        [Fact(DisplayName = "Asset Token Info: Topic Address for Asset Symbol Raises Error")]
        public async Task TopicAddressForTokenSymbolRaisesError()
        {
            await using var fx = await TestTopic.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(fx.Record.Topic);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
        }
        [Fact(DisplayName = "Asset Token Info: File Address for Asset Symbol Raises Error")]
        public async Task FileAddressForTokenSymbolRaisesError()
        {
            await using var fx = await TestFile.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fx.Client.GetTokenInfoAsync(fx.Record.File);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidTokenId", pex.Message);
        }
    }
}
