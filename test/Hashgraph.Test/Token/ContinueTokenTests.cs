#pragma warning disable CS0618 // Type or member is obsolete
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class ContinueTokenTests
{
    private readonly NetworkCredentials _network;
    public ContinueTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Continue Tokens: Can Reume Token Coin Trading")]
    public async Task CanReumeTokenCoinTrading()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

        await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Tokens: Can Continue Token Coin Trading and get Record")]
    public async Task CanReumeTokenCoinTradingAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

        var record = await fxToken.Client.ContinueTokenWithRecordAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Tokens: Can Continue Token Coin Trading and get Record (No Extra Signatory)")]
    public async Task CanReumeTokenCoinTradingAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

        var record = await fxToken.Client.ContinueTokenWithRecordAsync(fxToken.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.PausePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Tokens: Can Reume Token Coin Trading from Any Account with Suspend Key")]
    public async Task CanReumeTokenCoinTradingFromAnyAccountWithSuspendKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

        await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Tokens: Resuming an Unfrozen Account is Noop")]
    public async Task ResumingAnUnfrozenAccountIsNoop()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

        var info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.Equal(0Ul, info.Balance);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.False(info.AutoAssociated);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.Equal(xferAmount, info.Balance);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.False(info.AutoAssociated);
    }
    [Fact(DisplayName = "Continue Tokens: Can Continue a Paused Token")]
    public async Task CanContinueAPausedAccount()
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

        await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Tokens: Continue Token Requires Suspend Key to Sign Transaction")]
    public async Task ContinueTokenRequiresSuspendKeyToSignTransaction()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to Continue Token, status: InvalidSignature", tex.Message);

        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);
    }
    [Fact(DisplayName = "Continue Tokens: Can Not Continue Token when Pause Not Enabled")]
    public async Task CanNotContinueTokenWhenPauseNotEnabled()
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
            await fxToken.Client.ContinueTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Status);
        Assert.StartsWith("Unable to Continue Token, status: TokenHasNoPauseKey", tex.Message);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.NotApplicable);
    }
    [Fact(DisplayName = "Continue Tokens: Can Not Schedule Reume Token Coin Trading")]
    public async Task CanNotScheduleReumeTokenCoinTrading()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxToken.Client.PauseTokenAsync(fxToken.Record.Token, fxToken.PausePrivateKey);
        await AssertHg.TokenPausedAsync(fxToken, TokenTradableStatus.Suspended);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ContinueTokenAsync(
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