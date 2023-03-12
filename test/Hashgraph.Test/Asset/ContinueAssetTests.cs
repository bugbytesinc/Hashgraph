#pragma warning disable CS0618 // Type or member is obsolete
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken;

[Collection(nameof(NetworkCredentials))]
public class ContinueAssetTests
{
    private readonly NetworkCredentials _network;
    public ContinueAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Continue Assets: Can Reume Asset Coin Trading")]
    public async Task CanReumeAssetCoinTrading()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Can Reume Asset Coin Trading Without Extra Signatory")]
    public async Task CanReumeAssetCoinTradingWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, ctx => ctx.Signatory = new Signatory(ctx.Signatory, fxAsset.PausePrivateKey));

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, ctx => ctx.Signatory = new Signatory(ctx.Signatory, fxAsset.PausePrivateKey));

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Can Continue Asset Coin Trading and get Record")]
    public async Task CanReumeAssetCoinTradingAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var record = await fxAsset.Client.ContinueTokenWithRecordAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Can Continue Asset Coin Trading and get Record Without Signatory")]
    public async Task CanReumeAssetCoinTradingAndGetRecordWithoutSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var record = await fxAsset.Client.ContinueTokenWithRecordAsync(fxAsset.Record.Token, ctx => ctx.Signatory = new Signatory(ctx.Signatory, fxAsset.PausePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Can Continue Asset Coin Trading and get Record (No Extra Signatory)")]
    public async Task CanReumeAssetCoinTradingAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var record = await fxAsset.Client.ContinueTokenWithRecordAsync(fxAsset.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.PausePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Can Reume Asset Coin Trading from Any Account with Pause Key")]
    public async Task CanReumeAssetCoinTradingFromAnyAccountWithSuspendKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Continue Assets: Continuing an Unpaused Token is Noop")]
    public async Task ContinuingAnUnpausedTokenIsNoop()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        var circulation = (ulong)fxAsset.Metadata.Length;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        var info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(0Ul, info.Balance);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.False(info.AutoAssociated);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        info = (await fxAccount.Client.GetAccountInfoAsync(fxAccount)).Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(1UL, info.Balance);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.False(info.AutoAssociated);
    }
    [Fact(DisplayName = "Continue Assets: Continue Asset Requires Pause Key to Sign Transaction")]
    public async Task ContinueAssetRequiresSuspendKeyToSignTransaction()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to Continue Token, status: InvalidSignature", tex.Message);

        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);
    }
    [Fact(DisplayName = "Continue Assets: Can Not Continue Asset when Pause Not Enabled")]
    public async Task CanNotContinueAssetWhenPauseNotEnabled()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.PauseEndorsement = null;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.NotApplicable);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ContinueTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Status);
        Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Receipt.Status);
        Assert.StartsWith("Unable to Continue Token, status: TokenHasNoPauseKey", tex.Message);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.NotApplicable);
    }
    [Fact(DisplayName = "Continue Assets: Can Not Schedule Reume Asset Coin Trading")]
    public async Task CanNotScheduleReumeAssetCoinTrading()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);
        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ContinueTokenAsync(
                fxAsset.Record.Token,
                new Signatory(
                    fxAsset.PausePrivateKey,
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