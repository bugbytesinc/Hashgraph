using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken
{
    [Collection(nameof(NetworkCredentials))]
    public class RevokeAssetTests
    {
        private readonly NetworkCredentials _network;
        public RevokeAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Revoke Assets: Can Revoke Assets")]
        public async Task CanRevokeTokens()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await fxAsset.Client.RevokeTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
        }
        [Fact(DisplayName = "Revoke Assets: Can Revoke Assets and get Record")]
        public async Task CanRevokeTokensAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            var record = await fxAsset.Client.RevokeTokenKycWithRecordAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
        }
        [Fact(DisplayName = "Revoke Assets: Can Revoke Assets and get Record (Without Extra Signatory)")]
        public async Task CanRevokeTokensAndGetRecordWithoutExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            var record = await fxAsset.Client.RevokeTokenKycWithRecordAsync(fxAsset.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.GrantPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
        }
        [Fact(DisplayName = "Revoke Assets: Can Revoke Assets from any Account with Grant Key")]
        public async Task CanRevokeTokenCoinsFromAnyAccountWithGrantKey()
        {
            await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await fxAsset.Client.RevokeTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey, ctx =>
            {
                ctx.Payer = fxOther.Record.Address;
                ctx.Signatory = fxOther.PrivateKey;
            });

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 2), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
        }
        [Fact(DisplayName = "Revoke Assets: Revoke Assets Requires Grant Key Signature")]
        public async Task RevokeTokenCoinsRequiresGrantKeySignature()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.RevokeTokenKycAsync(fxAsset.Record.Token, fxAccount);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
            Assert.StartsWith("Unable to Revoke Token, status: InvalidSignature", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);
        }
        [Fact(DisplayName = "Revoke Assets: Cannot Revoke Assets When Grant KYC is Turned Off")]
        public async Task CannotRevokeTokenCoinsWhenGrantKycIsTurnedOff()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.NotApplicable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.RevokeTokenKycAsync(fxAsset.Record.Token, fxAccount, fxAsset.GrantPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Status);
            Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Receipt.Status);
            Assert.StartsWith("Unable to Revoke Token, status: TokenHasNoKycKey", tex.Message);
        }
        [Fact(DisplayName = "Revoke Assets: Can Not Schedule Revoke Assets")]
        public async Task CanNotScheduleRevokeTokenCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);
            var circulation = (ulong)fxAsset.Metadata.Length;

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Revoked);

            await fxAsset.Client.GrantTokenKycAsync(fxAsset, fxAccount, fxAsset.GrantPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenKycStatus.Granted);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.RevokeTokenKycAsync(
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
