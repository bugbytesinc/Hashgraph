﻿namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class RevokeTokenTests
{
    private readonly NetworkCredentials _network;
    public RevokeTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Revoke Tokens: Can Revoke Token Coins")]
    public async Task CanRevokeTokens()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        receipt = await fxToken.Client.RevokeTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Revoke Tokens: Can Revoke Token Coins from Alias Account")]
    public async Task CanRevokeTokensFromAliasAccountDefect()
    {
        // Associating an asset with an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanRevokeTokensFromAliasAccount));
        Assert.StartsWith("Unable to Revoke Token, status: InvalidAccountId", testFailException.Message);

        //[Fact(DisplayName = "Revoke Tokens: Can Revoke Token Coins from Alias Account")]
        async Task CanRevokeTokensFromAliasAccount()
        {
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);
            await fxToken.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount, fxAccount.PrivateKey);

            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

            var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

            await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

            receipt = await fxToken.Client.RevokeTokenKycAsync(fxToken.Record.Token, fxAccount.Alias, fxToken.GrantPrivateKey);

            await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
        }
    }
    [Fact(DisplayName = "Revoke Tokens: Can Revoke Token Coins and get Record")]
    public async Task CanRevokeTokensAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        var record = await fxToken.Client.RevokeTokenKycWithRecordAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
    }
    [Fact(DisplayName = "Revoke Tokens: Can Revoke Token Coins and get Record (Without Extra Signatory)")]
    public async Task CanRevokeTokensAndGetRecordWithoutExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        var record = await fxToken.Client.RevokeTokenKycWithRecordAsync(fxToken.Record.Token, fxAccount, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.GrantPrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
    }
    [Fact(DisplayName = "Revoke Tokens: Can Revoke Token Coins from any Account with Grant Key")]
    public async Task CanRevokeTokenCoinsFromAnyAccountWithGrantKey()
    {
        await using var fxOther = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        receipt = await fxToken.Client.RevokeTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey, ctx =>
        {
            ctx.Payer = fxOther.Record.Address;
            ctx.Signatory = fxOther.PrivateKey;
        });

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);
    }
    [Fact(DisplayName = "Revoke Tokens: Revoke Token Coins Requires Grant Key Signature")]
    public async Task RevokeTokenCoinsRequiresGrantKeySignature()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.RevokeTokenKycAsync(fxToken.Record.Token, fxAccount);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.StartsWith("Unable to Revoke Token, status: InvalidSignature", tex.Message);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);
    }
    [Fact(DisplayName = "Revoke Tokens: Cannot Revoke Token Coins When Grant KYC is Turned Off")]
    public async Task CannotRevokeTokenCoinsWhenGrantKycIsTurnedOff()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.NotApplicable);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.RevokeTokenKycAsync(fxToken.Record.Token, fxAccount, fxToken.GrantPrivateKey);
        });
        Assert.Equal(ResponseCode.TokenHasNoKycKey, tex.Status);
        Assert.StartsWith("Unable to Revoke Token, status: TokenHasNoKycKey", tex.Message);
    }
    [Fact(DisplayName = "Revoke Tokens: Can Not Schedule Revoke Token Coins")]
    public async Task CanNotScheduleRevokeTokenCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Revoked);

        var receipt = await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount, fxToken.GrantPrivateKey);

        await AssertHg.TokenKycStatusAsync(fxToken, fxAccount, TokenKycStatus.Granted);

        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.RevokeTokenKycAsync(
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