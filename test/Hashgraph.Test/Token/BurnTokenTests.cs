using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class BurnTokenTests
{
    private readonly NetworkCredentials _network;
    public BurnTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Burn Tokens: Can Burn Token Coins")]
    public async Task CanBurnTokensAsync()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        Assert.NotNull(fxToken.Record);
        Assert.NotNull(fxToken.Record.Token);
        Assert.True(fxToken.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var amountToDestory = fxToken.Params.Circulation / 3;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

        var receipt = await fxToken.Client.BurnTokenAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedCirculation, receipt.Circulation);


        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(expectedCirculation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        Assert.Equal(expectedCirculation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Burn Tokens: Can Burn Token Coins and get Record")]
    public async Task CanBurnTokensAsyncAndGetRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        Assert.NotNull(fxToken.Record);
        Assert.NotNull(fxToken.Record.Token);
        Assert.True(fxToken.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var amountToDestory = fxToken.Params.Circulation / 3;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

        var record = await fxToken.Client.BurnTokenWithRecordAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedCirculation, record.Circulation);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(expectedCirculation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        Assert.Equal(expectedCirculation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Burn Tokens: Can Burn Token Coins and get Record Without Extra Signatory")]
    public async Task CanBurnTokensAsyncAndGetRecordWithoutExtraSignatory()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        Assert.NotNull(fxToken.Record);
        Assert.NotNull(fxToken.Record.Token);
        Assert.True(fxToken.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var amountToDestory = fxToken.Params.Circulation / 3;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

        var record = await fxToken.Client.BurnTokenWithRecordAsync(fxToken, amountToDestory, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.SupplyPrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedCirculation, record.Circulation);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(expectedCirculation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        Assert.Equal(expectedCirculation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Burn Tokens: Can Burn Token Coins from Any Account with Supply Key")]
    public async Task CanBurnTokenCoinsFromAnyAccountWithSupplyKey()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 60_000_000_000);
        await using var fxToken = await TestToken.CreateAsync(_network);
        Assert.NotNull(fxToken.Record);
        Assert.NotNull(fxToken.Record.Token);
        Assert.True(fxToken.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var amountToDestory = fxToken.Params.Circulation / 3;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

        var receipt = await fxAccount.Client.BurnTokenAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedCirculation, receipt.Circulation);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(expectedCirculation, info.Circulation);
    }
    [Fact(DisplayName = "Burn Tokens: Burn Token Record Includes Token Transfers")]
    public async Task BurnTokenRecordIncludesTokenTransfers()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var amountToDestory = fxToken.Params.Circulation / 3;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;

        var record = await fxToken.Client.BurnTokenWithRecordAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
        Assert.Single(record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);
        Assert.Equal(expectedCirculation, record.Circulation);

        var xfer = record.TokenTransfers[0];
        Assert.Equal(fxToken.Record.Token, xfer.Token);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, xfer.Address);
        Assert.Equal(-(long)amountToDestory, xfer.Amount);

        Assert.Equal(expectedCirculation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(expectedCirculation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Burn Tokens: Can Not Burn More Tokens than are in Circulation")]
    public async Task CanNotBurnMoreTokensThanAreInCirculation()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        var amountToDestory = fxToken.Params.Circulation + 1;

        Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.BurnTokenAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
        });
        Assert.Equal(ResponseCode.InvalidTokenBurnAmount, tex.Status);
        Assert.StartsWith("Unable to Burn Token Coins, status: InvalidTokenBurnAmount", tex.Message);

        Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Burn Tokens: Burning Coins Requires Supply Key to Sign Transaction")]
    public async Task BurningCoinsRequiresSupplyKeyToSignTransaction()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        var amountToDestory = fxToken.Params.Circulation / 3;

        Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.BurnTokenAsync(fxToken, amountToDestory);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to Burn Token Coins, status: InvalidSignature", tex.Message);

        Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Burn Tokens: Can Not Burn More Tokens than Treasury Has")]
    public async Task CanNotBurnMoreTokensThanTreasuryHas()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        var amountToDestory = 2 * fxToken.Params.Circulation / 3;
        var amountToTransfer = amountToDestory;
        var expectedTreasury = fxToken.Params.Circulation - amountToTransfer;

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)amountToTransfer, fxToken.TreasuryAccount);

        Assert.Equal(amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
        Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.BurnTokenAsync(fxToken, amountToDestory, fxToken.SupplyPrivateKey);
        });
        Assert.Equal(ResponseCode.InsufficientTokenBalance, tex.Status);
        Assert.StartsWith("Unable to Burn Token Coins, status: InsufficientTokenBalance", tex.Message);

        Assert.Equal(amountToTransfer, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
        Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Burn Tokens: Can Schedule Burn Token Coins")]
    public async Task CanScheduleBurnTokenCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.InitialBalance = 40_00_000_000);
        await using var fxToken = await TestToken.CreateAsync(_network);
        var amountToDestory = fxToken.Params.Circulation / 3 + 1;
        var expectedCirculation = fxToken.Params.Circulation - amountToDestory;
        var pendingReceipt = await fxToken.Client.BurnTokenAsync(
                fxToken,
                amountToDestory,
                new Signatory(
                    fxToken.SupplyPrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    }));

        Assert.Equal(ResponseCode.Success, pendingReceipt.Status);
        // This should be considered a network bug.
        Assert.Equal(0UL, pendingReceipt.Circulation);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation);

        var signingReceipt = await fxPayer.Client.SignPendingTransactionAsync(pendingReceipt.Pending.Id, fxPayer.PrivateKey);
        Assert.Equal(ResponseCode.Success, signingReceipt.Status);

        var executedReceipt = await fxPayer.Client.GetReceiptAsync(pendingReceipt.Pending.TxId) as TokenReceipt;
        Assert.Equal(ResponseCode.Success, executedReceipt.Status);
        Assert.Equal(expectedCirculation, executedReceipt.Circulation);

        var executedRecord = await fxPayer.Client.GetTransactionRecordAsync(pendingReceipt.Pending.TxId) as TokenRecord;
        Assert.Equal(ResponseCode.Success, executedRecord.Status);
        Assert.Equal(expectedCirculation, executedRecord.Circulation);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, expectedCirculation);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(expectedCirculation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^
    }
}