namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class AutoAssociateTokenTests
{
    private readonly NetworkCredentials _network;
    public AutoAssociateTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Auto Associate Tokens: Can Auto Associate token with Account")]
    public async Task CanAutoAssociateTokenWithAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

        var xferAmount = fxToken.Params.Circulation / 2;
        var receipt = await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await _network.WaitForMirrorConsensusAsync(receipt);

        var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
        Assert.Equal(fxToken.Record.Token, association.Token);
        Assert.Equal((long)xferAmount, association.Balance);
        Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.True(association.AutoAssociated);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);

        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.Single(tokens);

        var balance = tokens[0];
        Assert.Equal(fxToken.Record.Token, balance.Token);
        Assert.Equal(TokenKycStatus.NotApplicable, balance.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, balance.FreezeStatus);
        Assert.True(balance.AutoAssociated);
    }
    [Fact(DisplayName = "Auto Associate Tokens: Can Auto Associate asset with Account")]
    public async Task CanAutoAssociateAssetWithAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

        var receipt = await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await _network.WaitForMirrorConsensusAsync(receipt);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(1, association.Balance);
        Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.True(association.AutoAssociated);

        var info = await fxAccount.Client.GetAccountInfoAsync(fxAccount);
        Assert.Equal(fxAccount.CreateParams.AutoAssociationLimit, info.AutoAssociationLimit);

        var tokens = await fxAccount.GetTokenBalancesAsync();
        Assert.Single(tokens);

        var balance = tokens[0];
        Assert.Equal(fxAsset.Record.Token, balance.Token);
        Assert.Equal(TokenKycStatus.NotApplicable, balance.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, balance.FreezeStatus);
        Assert.True(balance.AutoAssociated);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Limit Token Auto Association")]
    public async Task CanLimitTokenAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 1);
        await using var fxToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

        var xferAmount1 = fxToken1.Params.Circulation / 2;
        var receipt = await fxToken1.Client.TransferTokensAsync(fxToken1, fxToken1.TreasuryAccount, fxAccount, (long)xferAmount1, fxToken1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var xferAmount2 = fxToken2.Params.Circulation / 2;
            var receipt2 = await fxToken2.Client.TransferTokensAsync(fxToken2, fxToken2.TreasuryAccount, fxAccount, (long)xferAmount2, fxToken2.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: NoRemainingAutomaticAssociations", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        await AssertHg.TokenBalanceAsync(fxToken1, fxAccount, xferAmount1);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Limit Asset Auto Association")]
    public async Task CanLimitAssetAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 1);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

        var receipt = await fxAsset1.Client.TransferAssetAsync(new Asset(fxAsset1, 1), fxAsset1.TreasuryAccount, fxAccount, fxAsset1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset1.Client.TransferAssetAsync(new Asset(fxAsset2, 1), fxAsset2.TreasuryAccount, fxAccount, fxAsset2.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: NoRemainingAutomaticAssociations", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
        await AssertHg.AssetBalanceAsync(fxAsset1, fxAccount, 1);
    }

    [Fact(DisplayName = "Auto Associate Tokens: No Token Auto Association Results in Error")]
    public async Task NoTokenAutoAssociationResultsInError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 0);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var xferAmount2 = fxToken.Params.Circulation / 2;
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount2, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", tex.Message);

        await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
    }

    [Fact(DisplayName = "Auto Associate Tokens: No Asset Auto Association Results in Error")]
    public async Task NoAssetAutoAssociationResultsInError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 0);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount, fxAccount, fxAsset.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", tex.Message);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Lower Limit Token Auto Association")]
    public async Task CanLowerLimitTokenAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 100);
        await using var fxToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

        var xferAmount1 = fxToken1.Params.Circulation / 2;
        var receipt = await fxToken1.Client.TransferTokensAsync(fxToken1, fxToken1.TreasuryAccount, fxAccount, (long)xferAmount1, fxToken1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = 1,
            Signatory = fxAccount
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var xferAmount2 = fxToken2.Params.Circulation / 2;
            var receipt2 = await fxToken2.Client.TransferTokensAsync(fxToken2, fxToken2.TreasuryAccount, fxAccount, (long)xferAmount2, fxToken2.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: NoRemainingAutomaticAssociations", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        await AssertHg.TokenBalanceAsync(fxToken1, fxAccount, xferAmount1);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Lower Limit Asset Auto Association")]
    public async Task CanLowerLimitAssetAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 100);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

        var receipt = await fxAsset1.Client.TransferAssetAsync(new Asset(fxAsset1, 1), fxAsset1.TreasuryAccount, fxAccount, fxAsset1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = 1,
            Signatory = fxAccount
        });

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset1.Client.TransferAssetAsync(new Asset(fxAsset2, 1), fxAsset2.TreasuryAccount, fxAccount, fxAsset2.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: NoRemainingAutomaticAssociations", tex.Message);

        await _network.WaitForMirrorConsensusAsync(tex);

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
        await AssertHg.AssetBalanceAsync(fxAsset1, fxAccount, 1);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Raise Limit Token Auto Association")]
    public async Task CanRaiseLimitTokenAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 1);
        await using var fxToken1 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxToken2 = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await _network.WaitForMirrorConsensusAsync();

        await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

        var xferAmount1 = fxToken1.Params.Circulation / 2;
        var receipt = await fxToken1.Client.TransferTokensAsync(fxToken1, fxToken1.TreasuryAccount, fxAccount, (long)xferAmount1, fxToken1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = 2,
            Signatory = fxAccount
        });

        var xferAmount2 = fxToken2.Params.Circulation / 2;
        var receipt2 = await fxToken2.Client.TransferTokensAsync(fxToken2, fxToken2.TreasuryAccount, fxAccount, (long)xferAmount2, fxToken2.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt2.Status);

        await _network.WaitForMirrorConsensusAsync(receipt2);

        await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
        await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);
        await AssertHg.TokenBalanceAsync(fxToken1, fxAccount, xferAmount1);
        await AssertHg.TokenBalanceAsync(fxToken2, fxAccount, xferAmount2);
    }

    [Fact(DisplayName = "Auto Associate Tokens: Can Raise Limit Asset Auto Association")]
    public async Task CanRaiseLimitAssetAutoAssociation()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 100);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

        var receipt = await fxAsset1.Client.TransferAssetAsync(new Asset(fxAsset1, 1), fxAsset1.TreasuryAccount, fxAccount, fxAsset1.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await fxAccount.Client.UpdateAccountAsync(new UpdateAccountParams
        {
            Address = fxAccount,
            AutoAssociationLimit = 2,
            Signatory = fxAccount
        });

        var receipt2 = await fxAsset2.Client.TransferAssetAsync(new Asset(fxAsset2, 1), fxAsset2.TreasuryAccount, fxAccount, fxAsset2.TreasuryAccount);
        Assert.Equal(ResponseCode.Success, receipt2.Status);

        await _network.WaitForMirrorConsensusAsync(receipt2);

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);
        await AssertHg.AssetBalanceAsync(fxAsset1, fxAccount, 1);
        await AssertHg.AssetBalanceAsync(fxAsset2, fxAccount, 1);
    }
}