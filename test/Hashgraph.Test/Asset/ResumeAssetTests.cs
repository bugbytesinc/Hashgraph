namespace Hashgraph.Test.AssetToken;

[Collection(nameof(NetworkCredentials))]
public class ResumeAssetTests
{
    private readonly NetworkCredentials _network;
    public ResumeAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Resume Assets: Can Reume Asset Coin Trading")]
    public async Task CanReumeAssetCoinTrading()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var receipt = await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Resume Assets: Can Resume Asset Coin Trading and get Record")]
    public async Task CanReumeAssetCoinTradingAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var record = await fxAsset.Client.ResumeTokenWithRecordAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await _network.WaitForMirrorConsensusAsync(record);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        var receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Resume Assets: Can Resume Asset Coin Trading and get Record (No Extra Signatory)")]
    public async Task CanReumeAssetCoinTradingAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var record = await fxAsset.Client.ResumeTokenWithRecordAsync(fxAsset.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxAsset.SuspendPrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await _network.WaitForMirrorConsensusAsync(record);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        var receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Resume Assets: Can Reume Asset Coin Trading from Any Account with Suspend Key")]
    public async Task CanReumeAssetCoinTradingFromAnyAccountWithSuspendKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var receipt = await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Resume Assets: Resuming an Unfrozen Account is Noop")]
    public async Task ResumingAnUnfrozenAccountIsNoop()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = false;
        }, fxAccount);
        var circulation = (ulong)fxAsset.Metadata.Length;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        var receipt = await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

        await _network.WaitForMirrorConsensusAsync(receipt);

        var info = (await fxAccount.GetTokenBalancesAsync()).FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(0, info.Balance);
        Assert.Equal(TokenTradableStatus.Tradable, info.FreezeStatus);
        Assert.False(info.AutoAssociated);

        receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        info = (await fxAccount.GetTokenBalancesAsync()).FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(1, info.Balance);
        Assert.Equal(TokenTradableStatus.Tradable, info.FreezeStatus);
        Assert.False(info.AutoAssociated);
    }
    [Fact(DisplayName = "Resume Assets: Can Resume a Suspended Account")]
    public async Task CanResumeASuspendedAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = false;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        var receipt = await fxAsset.Client.SuspendTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        receipt = await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);

        receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await _network.WaitForMirrorConsensusAsync(receipt);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Tradable);
    }
    [Fact(DisplayName = "Resume Assets: Resume Asset Requires Suspend Key to Sign Transaction")]
    public async Task ResumeAssetRequiresSuspendKeyToSignTransaction()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to Resume Token, status: InvalidSignature", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        var info = (await fxAccount.GetTokenBalancesAsync()).FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(0, info.Balance);
        Assert.Equal(TokenTradableStatus.Suspended, info.FreezeStatus);
        Assert.False(info.AutoAssociated);

        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);
    }
    [Fact(DisplayName = "Resume Assets: Can Not Resume Asset when Freeze Not Enabled")]
    public async Task CanNotResumeAssetWhenFreezeNotEnabled()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.SuspendEndorsement = null;
            fx.Params.InitializeSuspended = false;
        }, fxAccount);
        var circulation = fxAsset.Metadata.Length;
        var xferAmount = circulation / 3;

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.NotApplicable);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ResumeTokenAsync(fxAsset.Record.Token, fxAccount, fxAsset.SuspendPrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoFreezeKey, tex.Status);
        Assert.Equal(ResponseCode.TokenHasNoFreezeKey, tex.Receipt.Status);
        Assert.StartsWith("Unable to Resume Token, status: TokenHasNoFreezeKey", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        var info = (await fxAccount.GetTokenBalancesAsync()).FirstOrDefault(t => t.Token == fxAsset.Record.Token);
        Assert.Equal(0, info.Balance);
        Assert.Equal(TokenTradableStatus.NotApplicable, info.FreezeStatus);
        Assert.False(info.AutoAssociated);

        await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.NotApplicable);
    }
    [Fact(DisplayName = "Resume Assets: Can Not Schedule Reume Asset Coin Trading")]
    public async Task CanNotScheduleReumeAssetCoinTrading()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = true;
        }, fxAccount);

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.AssetStatusAsync(fxAsset, fxAccount, TokenTradableStatus.Suspended);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.ResumeTokenAsync(
                fxAsset.Record.Token,
                fxAccount,
                new Signatory(
                    fxAsset.SuspendPrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    }));
        });

        await _network.WaitForMirrorConsensusAsync(tex);

        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}