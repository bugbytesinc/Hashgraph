using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken
{
    [Collection(nameof(NetworkCredentials))]
    public class GrantAssetTests
    {
        private readonly NetworkCredentials _network;
        public GrantAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Grant Assets: Can Grant Asset Coins")]
        public async Task CanGrantAssets()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
        }
        [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Grant Assets: Can Grant Asset Coins to Alias Account")]
        public async Task CanGrantAssetsToAliasAccountDefect()
        {
            // Granting Access to an asset with an account using its alias address has not yet been
            // implemented by the network, although it will accept the transaction.
            var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanGrantAssetsToAliasAccount));
            Assert.StartsWith("Unable to Grant Token, status: InvalidAccountId", testFailException.Message);

            //[Fact(DisplayName = "Grant Assets: Can Grant Asset Coins to Alias Account")]
            async Task CanGrantAssetsToAliasAccount()
            {
                await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
                await using var fxAsset = await TestAsset.CreateAsync(_network);
                await fxAsset.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount, fxAccount.PrivateKey);

                await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

                await fxAsset.Client.GrantTokenKycAsync(fxAsset.Record.Token, fxAccount.Alias, fxAsset.GrantPrivateKey);

                await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

                await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
            }
        }
        [Fact(DisplayName = "Grant Assets: Can Grant Asset Coins and get Record")]
        public async Task CanGrantAssetsAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var record = await fxAsset.Client.GrantTokenKycWithRecordAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Null(record.ParentTransactionConcensus);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
        }
        [Fact(DisplayName = "Grant Assets: Can Grant Asset Coins and get Record (Without Extra Signatory)")]
        public async Task CanGrantAssetsAndGetRecordWithoutExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var record = await fxAsset.Client.GrantTokenKycWithRecordAsync(fxAsset.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.GrantPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
        }
        [Fact(DisplayName = "Grant Assets: Can Grant Asset Coins from any Account with Grant Key")]
        public async Task CanGrantAssetCoinsFromWnyAccountWithGrantKey()
        {
            await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey, ctx =>
            {
                ctx.Payer = fxOther.Record.Address;
                ctx.Signatory = fxOther.PrivateKey;
            });

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
        }
        [Fact(DisplayName = "Grant Assets: Grant Asset Coins Requires Grant Key Signature")]
        public async Task GrantAssetCoinsRequiresGrantKeySignature()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.GrantTokenKycAsync(fxAsset.Record.Token, fxAccount);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
            Assert.StartsWith("Unable to Grant Token, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Grant Assets: Cannot Grant Asset Coins When Grant KYC Turned Off")]
        public async Task CannotGrantAssetCoinsWhenGrantKYCTurnedOff()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.NotApplicable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.GrantTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Status);
            Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Receipt.Status);
            Assert.StartsWith("Unable to Grant Token, status: TokenHasNoKycKey", tex.Message);
        }
        [Fact(DisplayName = "Grant Assets: Can Not Schedule Grant Asset Coins")]
        public async Task CanNotScheduleGrantAssetCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = fxAsset.Metadata.Length;
            var xferAmount = circulation / 3;
            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.GrantTokenKycAsync(
                    fxAsset.Record.Token,
                    fxAccount,
                    new Signatory(
                        fxAsset.GrantPrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
