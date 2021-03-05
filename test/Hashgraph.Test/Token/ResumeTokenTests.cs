using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class ResumeTokenTests
    {
        private readonly NetworkCredentials _network;
        public ResumeTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Resume Tokens: Can Reume Token Coin Trading")]
        public async Task CanReumeTokenCoinTrading()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Resume Tokens: Can Resume Token Coin Trading and get Record")]
        public async Task CanReumeTokenCoinTradingAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            var record = await fxToken.Client.ResumeTokenWithRecordAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Resume Tokens: Can Resume Token Coin Trading and get Record (No Extra Signatory)")]
        public async Task CanReumeTokenCoinTradingAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            var record = await fxToken.Client.ResumeTokenWithRecordAsync(fxToken.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.SuspendPrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Resume Tokens: Can Reume Token Coin Trading from Any Account with Suspend Key")]
        public async Task CanReumeTokenCoinTradingFromAnyAccountWithSuspendKey()
        {
            await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey, ctx =>
            {
                ctx.Payer = fxOther.Record.Address;
                ctx.Signatory = fxOther.PrivateKey;
            });

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Resume Tokens: Resuming an Unfrozen Account is Noop")]
        public async Task ResumingAnUnfrozenAccountIsNoop()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);

            var info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Equal(0Ul, info.Balance);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Equal(xferAmount, info.Balance);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        }
        [Fact(DisplayName = "Resume Tokens: Can Resume a Suspended Account")]
        public async Task CanResumeASuspendedAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.SuspendTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Tradable);
        }
        [Fact(DisplayName = "Resume Tokens: Resume Token Requires Suspend Key to Sign Transaction")]
        public async Task ResumeTokenRequiresSuspendKeyToSignTransaction()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Resume Token, status: InvalidSignature", tex.Message);

            var info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Equal(0Ul, info.Balance);
            Assert.Equal(TokenTradableStatus.Suspended, info.TradableStatus);

            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);
        }
        [Fact(DisplayName = "Resume Tokens: Can Not Resume Token when Freeze Not Enabled")]
        public async Task CanNotResumeTokenWhenFreezeNotEnabled()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.SuspendEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.NotApplicable);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount, fxToken.SuspendPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenHasNoFreezeKey, tex.Status);
            Assert.StartsWith("Unable to Resume Token, status: TokenHasNoFreezeKey", tex.Message);

            var info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Equal(0Ul, info.Balance);
            Assert.Equal(TokenTradableStatus.NotApplicable, info.TradableStatus);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.NotApplicable);
        }
        [Fact(DisplayName = "Resume Tokens: Can Not Schedule Reume Token Coin Trading")]
        public async Task CanNotScheduleReumeTokenCoinTrading()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount);
            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenTradableStatus.Suspended);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.ResumeTokenAsync(
                    fxToken.Record.Token,
                    fxAccount,
                    new Signatory(
                        fxToken.SuspendPrivateKey,
                        new ScheduleParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.UnschedulableTransaction, tex.Status);
            Assert.StartsWith("Unable to Resume Token, status: UnschedulableTransaction", tex.Message);
        }
    }
}
