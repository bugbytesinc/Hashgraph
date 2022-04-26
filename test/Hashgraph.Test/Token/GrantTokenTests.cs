using Hashgraph.Test.Fixtures;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class GrantTokenTests
{
    private readonly NetworkCredentials _network;
    public GrantTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Grant Tokens: Can Grant Token Coins")]
    public async Task CanGrantTokens()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Grant Tokens: Can Grant Token Coins to Alias Account")]
    public async Task CanGrantTokensToAliasAccountDefect()
    {
        // Associating an asset with an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanGrantTokensToAliasAccount));
        Assert.StartsWith("Unable to Grant Token, status: InvalidAccountId", testFailException.Message);

        //[Fact(DisplayName = "Grant Tokens: Can Grant Token Coins to Alias Account")]
        async Task CanGrantTokensToAliasAccount()
        {
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);
            await fxToken.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount, fxAccount.PrivateKey);

            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

            await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount.Alias, fxToken.GrantPrivateKey);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
        }
    }
    [Fact(DisplayName = "Grant Tokens: Can Grant Token Coins and get Record")]
    public async Task CanGrantTokensAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var record = await fxToken.Client.GrantTokenKycWithRecordAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
    }
    [Fact(DisplayName = "Grant Tokens: Can Grant Token Coins and get Record (Without Extra Signatory)")]
    public async Task CanGrantTokensAndGetRecordWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var record = await fxToken.Client.GrantTokenKycWithRecordAsync(fxToken.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.GrantPrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
    }
    [Fact(DisplayName = "Grant Tokens: Can Grant Token Coins from any Account with Grant Key")]
    public async Task CanGrantTokenCoinsFromWnyAccountWithGrantKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
    }
    [Fact(DisplayName = "Grant Tokens: Grant Token Coins Requires Grant Key Signature")]
    public async Task GrantTokenCoinsRequiresGrantKeySignature()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network);

        await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to Grant Token, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Grant Tokens: Cannot Grant Token Coins When Grant KYC Turned Off")]
    public async Task CannotGrantTokenCoinsWhenGrantKYCTurnedOff()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);

        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.NotApplicable);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Status);
        Assert.StartsWith("Unable to Grant Token, status: TokenHasNoKycKey", tex.Message);
    }
    [Fact(DisplayName = "Grant Tokens: Cannot Grant Token Coins Before Auto Association")]
    public async Task CannotGrantTokenCoinsBeforeAutoAssociation()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount2);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount1, fxToken.GrantPrivateKey);
        });
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        Assert.StartsWith("Unable to Grant Token, status: TokenNotAssociatedToAccount", tex.Message);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken.Record.Token, fxAccount2, fxToken.GrantPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);
    }
    [Fact(DisplayName = "Grant Tokens: Can Not Schedule Grant Token Coins")]
    public async Task CanNotScheduleGrantTokenCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;
        await AssertHg.TokenStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.GrantTokenKycAsync(
                fxToken.Record.Token,
                fxAccount,
                new Signatory(
                    fxToken.GrantPrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    }));
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}