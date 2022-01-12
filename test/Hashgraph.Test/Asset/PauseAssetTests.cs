using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken;

[Collection(nameof(NetworkCredentials))]
public class PauseAssetTests
{
    private readonly NetworkCredentials _network;
    public PauseAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Pause Assets: Can Pause Asset Trading")]
    public async Task CanPauseAssetTrading()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);
    }
    [Fact(DisplayName = "Pause Assets: Can Pause Asset Trading and get Record")]
    public async Task CanPauseAssetTradingAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        var record = await fxAsset.Client.PauseTokenWithRecordAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);
    }
    [Fact(DisplayName = "Pause Assets: Can Pause Asset Trading and get Record (No Extra Signatory)")]
    public async Task CanPauseAssetTradingAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        var record = await fxAsset.Client.PauseTokenWithRecordAsync(fxAsset.Record.Token, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.PausePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);
    }
    [Fact(DisplayName = "Pause Assets: Can Pause Asset Trading from Any Account with Pause Key")]
    public async Task CanPauseAssetTradingFromAnyAccountWithPauseKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);
    }
    [Fact(DisplayName = "Pause Assets: Pauseing a Frozen Account is a Noop")]
    public async Task PauseingAFrozenAccountIsANoop()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        }, fxAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);

        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Status);
        Assert.Equal(ResponseCode.TokenIsPaused, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenIsPaused", tex.Message);
    }
    [Fact(DisplayName = "Pause Assets: Pause Asset Requires Pause Key to Sign Transaciton")]
    public async Task PauseAssetRequiresPauseKeyToSignTransaciton()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to Pause Token, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Pause Assets: Cannot Pause Asset when Freeze Not Enabled")]
    public async Task CannotPauseAssetWhenFreezeNotEnabled()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.PauseEndorsement = null;
        });

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.NotApplicable);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.PauseTokenAsync(fxAsset.Record.Token, fxAsset.PausePrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Status);
        Assert.Equal(ResponseCode.TokenHasNoPauseKey, tex.Receipt.Status);
        Assert.StartsWith("Unable to Pause Token, status: TokenHasNoPauseKey", tex.Message);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.NotApplicable);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.NotApplicable);
    }
    [Fact(DisplayName = "Pause Assets: Can Not Schedule Pause Asset Trading")]
    public async Task CanNotSchedulePauseAssetTrading()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
        });

        await AssertHg.AssetPausedAsync(fxAsset, TokenTradableStatus.Tradable);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.PauseTokenAsync(
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