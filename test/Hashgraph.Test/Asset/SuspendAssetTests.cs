using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken
{
    [Collection(nameof(NetworkCredentials))]
    public class SuspendAssetTests
    {
        private readonly NetworkCredentials _network;
        public SuspendAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Suspend Assets: Can Suspend Asset Trading")]
        public async Task CanSuspendAssetTrading()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Suspend Assets: Can Suspend Asset Trading and get Record")]
        public async Task CanSuspendAssetTradingAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

            var record = await fxAsset.Client.SuspendTokenWithRecordAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Suspend Assets: Can Suspend Asset Trading and get Record (No Extra Signatory)")]
        public async Task CanSuspendAssetTradingAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

            var record = await fxAsset.Client.SuspendTokenWithRecordAsync(fxAsset.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.SuspendPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Suspend Assets: Can Suspend Asset Trading from Any Account with Suspend Key")]
        public async Task CanSuspendAssetTradingFromAnyAccountWithSuspendKey()
        {
            await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey, ctx =>
            {
                ctx.Payer = fxOther.Record.Address;
                ctx.Signatory = fxOther.PrivateKey;
            });

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Suspend Assets: Suspending a Frozen Account is a Noop")]
        public async Task SuspendingAFrozenAccountIsANoop()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);
        }
        [Fact(DisplayName = "Suspend Assets: Can Suspend a Resumed Account")]
        public async Task CanSuspendAResumedAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

            await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Suspend Assets: Suspend Asset Requires Suspend Key to Sign Transaciton")]
        public async Task SuspendAssetRequiresSuspendKeyToSignTransaciton()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            });

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
            Assert.StartsWith("Unable to Suspend Token, status: InvalidSignature", tex.Message);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
        }
        [Fact(DisplayName = "Suspend Assets: Cannot Suspend Asset when Freeze Not Enabled")]
        public async Task CannotSuspendAssetWhenFreezeNotEnabled()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.SuspendEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.NotApplicable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenHasNoFreezeKey, tex.Status);
            Assert.Equal(ResponseCode.TokenHasNoFreezeKey, tex.Receipt.Status);
            Assert.StartsWith("Unable to Suspend Token, status: TokenHasNoFreezeKey", tex.Message);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.NotApplicable);

            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.NotApplicable);
        }
        [Fact(DisplayName = "Suspend Assets: Can Not Schedule Suspend Asset Trading")]
        public async Task CanNotScheduleSuspendAssetTrading()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);

            await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.SuspendTokenAsync(
                    fxAsset.Record.Token,
                    fxAccount,
                    new Signatory(
                        fxAsset.SuspendPrivateKey,
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
