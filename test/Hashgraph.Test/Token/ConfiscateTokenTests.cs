namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class ConfiscateTokenTests
{
    private readonly NetworkCredentials _network;
    public ConfiscateTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate Token Coins")]
    public async Task CanConfiscateTokenCoins()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2 * fxToken.Params.Circulation / (ulong)Generator.Integer(3, 5);
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var xferReceipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var receipt = await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate Token Coins From Alias Account")]
    public async Task CanConfiscateTokenCoinsFromAliasAccount()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await fxToken.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount, fxAccount.PrivateKey);

        var xferAmount = 2 * fxToken.Params.Circulation / (ulong)Generator.Integer(3, 5);
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var xferRecipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var receipt = await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount.Alias, xferAmount, fxToken.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }

    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate A Small Amount Token Coins")]
    public async Task CanConfiscateASmallAmountTokenCoins()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var xferReceipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var receipt = await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate A Small Amount Token Coins and get Record")]
    public async Task CanConfiscateASmallAmountTokenCoinsAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var record = await fxToken.Client.ConfiscateTokensWithRecordAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedTreasury, record.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate A Small Amount Token Coins and get Record without Extra Signatory")]
    public async Task CanConfiscateASmallAmountTokenCoinsAndGetRecordWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var record = await fxToken.Client.ConfiscateTokensWithRecordAsync(fxToken, fxAccount, xferAmount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.ConfiscatePrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(expectedTreasury, record.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Confiscate A Small Amount Token Coins from Any Account with Confiscate Key")]
    public async Task CanConfiscateASmallAmountTokenCoinsFromAnyAccountWithConfiscateKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var xferReceipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var receipt = await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.Equal(expectedTreasury, receipt.Circulation);

        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(expectedTreasury, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Not Confiscate More Tokens than Account Has")]
    public async Task CanNotConfiscateMoreTokensThanAccountHas()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount * 2, fxToken.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.InvalidWipingAmount, tex.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: InvalidWipingAmount", tex.Message);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Confiscate Record Includes Token Transfers")]
    public async Task ConfiscateRecordIncludesTokenTransfers()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var record = await fxToken.Client.ConfiscateTokensWithRecordAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(expectedTreasury, record.Circulation);
        Assert.Single(record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

        var xfer = record.TokenTransfers[0];
        Assert.Equal(fxToken.Record.Token, xfer.Token);
        Assert.Equal(fxAccount.Record.Address, xfer.Address);
        Assert.Equal(-(long)xferAmount, xfer.Amount);
        Assert.Equal(fxToken.Params.Circulation - xferAmount, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Confiscation Requires Confiscate Key Signature")]
    public async Task ConfiscationRequiresConfiscateKeySignature()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: InvalidSignature", tex.Message);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Cannot Confiscate when no Confiscation Endorsement")]
    public async Task CannotConfiscateWhenNoConfiscationEndorsement()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.ConfiscateEndorsement = null;
        }, fxAccount);
        var xferAmount = 2ul;
        var expectedTreasury = fxToken.Params.Circulation - xferAmount;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ConfiscateTokensAsync(fxToken, fxAccount, xferAmount, fxToken.ConfiscatePrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoWipeKey, tex.Status);
        Assert.StartsWith("Unable to Confiscate Token, status: TokenHasNoWipeKey", tex.Message);

        Assert.Equal((long)xferAmount, await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)expectedTreasury, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(fxToken.Params.Circulation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
    }
    [Fact(DisplayName = "Confiscate Tokens: Can Not Schedule Confiscate Token Coins")]
    public async Task CanNotScheduleConfiscateTokenCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var xferAmount = 2 * fxToken.Params.Circulation / (ulong)Generator.Integer(3, 5);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.ConfiscateTokensAsync(
                fxToken,
                fxAccount,
                xferAmount,
                new Signatory(
                    fxToken.ConfiscatePrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    })
            );
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}