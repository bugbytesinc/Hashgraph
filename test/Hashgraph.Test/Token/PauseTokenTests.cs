using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class PauseTokenTests
    {
        private readonly NetworkCredentials _network;
        public PauseTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Pause Tokens: Can Pause Token Coin Trading")]
        public async Task CanPauseTokenCoinTrading()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Pause Tokens: Can Pause Token Coin Trading and get Record")]
        public async Task CanPauseTokenCoinTradingAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            var record = await fxToken.Client.PauseTokenWithRecordAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Pause Tokens: Can Pause Token Coin Trading and get Record (No Extra Signatory)")]
        public async Task CanPauseTokenCoinTradingAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            var record = await fxToken.Client.PauseTokenWithRecordAsync(fxToken.Record.Token, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.PausePrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Pause Tokens: Can Pause Token Coin Trading from Any Account with Pause Key")]
        public async Task CanPauseTokenCoinTradingFromAnyAccountWithPauseKey()
        {
            await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey, ctx =>
            {
                ctx.Payer = fxOther.Record.Address;
                ctx.Signatory = fxOther.PrivateKey;
            });

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);
        }
        [Fact(DisplayName = "Pause Tokens: Pausing a Paused Token is a Noop")]
        public async Task PausingAPausedTokenIsANoop()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

            await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);
        }
        [Fact(DisplayName = "Pause Tokens: Pause Token Requires Pause Key to Sign Transaciton")]
        public async Task PauseTokenRequiresPauseKeyToSignTransaciton()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            });
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.PauseTokenAsync(fxToken.Record.Token);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Pause Token, status: InvalidSignature", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Pause Tokens: Cannot Pause Token when Pause Not Enabled")]
        public async Task CannotPauseTokenWhenFreezeNotEnabled()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.PauseEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.NotApplicable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
            });
            Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Status);
            Assert.StartsWith("Unable to Pause Token, status: TokenHasNoPauseKey", tex.Message);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.NotApplicable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.NotApplicable);
        }
        [Fact(DisplayName = "Pause Tokens: Can Not Schedule Pause Token Coin Trading")]
        public async Task CanNotSchedulePauseTokenCoinTrading()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;
            await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.PauseTokenAsync(
                    fxToken.Record.Token,
                    new Signatory(
                        fxToken.PausePrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
