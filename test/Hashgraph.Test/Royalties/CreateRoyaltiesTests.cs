namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class CreateRoyaltiesTests
{
    private readonly NetworkCredentials _network;
    public CreateRoyaltiesTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Royalties: Can Create Token with Fixed Royalty")]
    public async Task CanCreateTokenWithFixedRoyalty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxAccount, comToken, 100)
            };
        }, fxAccount);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fxToken.Params.Royalties.First(), info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Create Token with Fractional Royalty")]
    public async Task CanCreateTokenWithFractionalRoyalty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fxAccount, 1, 2, 1, 100)
            };
            fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fxToken.Params.Royalties.First(), info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Create Token with Fixed and Fractional Royalties")]
    public async Task CanCreateTokenWithFixedAndFractionalRoyalties()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
        var fixedRoyalty = new FixedRoyalty(fxAccount, comToken, 100);
        var fractionalRoyalty = new TokenRoyalty(fxAccount, 1, 2, 1, 100);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new IRoyalty[] { fixedRoyalty, fractionalRoyalty };
            fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(2, info.Royalties.Count);

        Assert.Equal(fixedRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Fixed));
        Assert.Equal(fractionalRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Token));
    }
    [Fact(DisplayName = "Royalties: Can Add Fixed Royalty to Token Definition")]
    public async Task CanAddFixedRoyaltyToTokenDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var fixedRoyalties = new FixedRoyalty[] { new FixedRoyalty(fxAccount, comToken, 100) };
        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, fixedRoyalties, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fixedRoyalties[0], info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Add Fractional Royalty to Token Definition")]
    public async Task CanAddFractionalRoyaltyToTokenDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var fractionalRoyalties = new TokenRoyalty[] { new TokenRoyalty(fxAccount, 1, 2, 1, 100) };
        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, fractionalRoyalties, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fractionalRoyalties[0], info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Add Fixed and Fractional Royalties to Token Definition")]
    public async Task CanAddFixedAndFractionalRoyaltiesToTokenDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var fixedRoyalty = new FixedRoyalty(fxAccount, comToken, 100);
        var fractionalRoyalty = new TokenRoyalty(fxAccount, 1, 2, 1, 100);
        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new IRoyalty[] { fixedRoyalty, fractionalRoyalty }, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(2, info.Royalties.Count);

        Assert.Equal(fixedRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Fixed));
        Assert.Equal(fractionalRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Token));
    }
    [Fact(DisplayName = "Royalties: Can Add Fixed and Fractional Royalties to Token Definition with Signatory In Context")]
    public async Task CanAddFixedAndFractionalRoyaltiesToTokenDefinitionWithSignatoryInContext()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comToken = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var fixedRoyalty = new FixedRoyalty(fxAccount, comToken, 100);
        var fractionalRoyalty = new TokenRoyalty(fxAccount, 1, 2, 1, 100);
        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, new IRoyalty[] { fixedRoyalty, fractionalRoyalty }, ctx => ctx.Signatory = new Signatory(ctx.Signatory, fxToken.RoyaltiesPrivateKey));
        Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(2, info.Royalties.Count);

        Assert.Equal(fixedRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Fixed));
        Assert.Equal(fractionalRoyalty, info.Royalties.First(f => f.RoyaltyType == RoyaltyType.Token));
    }
    [Fact(DisplayName = "Royalties: Can Create Asset with Fixed Royalty")]
    public async Task CanCreateAssetWithFixedRoyalty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxAccount, comAsset, 100)
            };
        }, fxAccount);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fxAsset.Params.Royalties.First(), info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Create Asset with Value Royalty")]
    public async Task CanCreateAssetWithValueRoyalty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new AssetRoyalty[]
            {
                    new AssetRoyalty(fxAccount, 1, 2, 1, comAsset.Record.Token)
            };
        }, fxAccount);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fxAsset.Params.Royalties.First(), info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Add Fixed Royalty to Asset Definition")]
    public async Task CanAddFixedRoyaltyToAssetDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var comAsset = await TestToken.CreateAsync(_network, null, fxAccount);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var fixedRoyalties = new FixedRoyalty[] { new FixedRoyalty(fxAccount, comAsset, 100) };
        var receipt = await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, fixedRoyalties, fxAsset.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Single(info.Royalties);

        Assert.Equal(fixedRoyalties[0], info.Royalties[0]);
    }
    [Fact(DisplayName = "Royalties: Can Not Add Fractional Royalty to Asset Definition")]
    public async Task CanNotAddFractionalRoyaltyToAssetDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var fractionalRoyalties = new TokenRoyalty[] { new TokenRoyalty(fxAccount, 1, 2, 1, 100) };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, fractionalRoyalties, fxAsset.RoyaltiesPrivateKey);
        });
        Assert.Equal(ResponseCode.CustomFractionalFeeOnlyAllowedForFungibleCommon, tex.Status);
        Assert.StartsWith("Unable to Update Royalties, status: CustomFractionalFeeOnlyAllowedForFungibleCommon", tex.Message);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Empty(info.Royalties);
    }
    [Fact(DisplayName = "Royalties: Can Not Create Token with Royalty Royalty")]
    public async Task CanNotCreateTokenWithRoyaltyRoyalty()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Royalties = new AssetRoyalty[]
                {
                        new AssetRoyalty(fxAccount, 1, 2, 0, Address.None)
                };
                fx.Params.Signatory = new Signatory(fx.Params.Signatory, fxAccount.PrivateKey);
            });
        });
        Assert.Equal(ResponseCode.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique, tex.Status);
        Assert.StartsWith("Unable to create Token, status: CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique", tex.Message);
    }
    [Fact(DisplayName = "Royalties: Can Not Add Royalty Royalty to Token Definition")]
    public async Task CanNotAddRoyaltyRoyaltyToTokenDefinition()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestToken.CreateAsync(_network, null, fxAccount);

        var valueRoyalties = new AssetRoyalty[] { new AssetRoyalty(fxAccount, 1, 2, 0, Address.None) };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.UpdateRoyaltiesAsync(fxAsset, valueRoyalties, fxAsset.RoyaltiesPrivateKey);
        });
        Assert.Equal(ResponseCode.CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique, tex.Status);
        Assert.StartsWith("Unable to Update Royalties, status: CustomRoyaltyFeeOnlyAllowedForNonFungibleUnique", tex.Message);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Empty(info.Royalties);
    }
    [Fact(DisplayName = "Royalties: Dissasociating as Fee Receiver Prevents Transfers")]
    public async Task DissasociatingAsFeeReceiverPreventsTransfers()
    {
        await using var fxReceiver = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var comToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxReceiver, fxAccount1, fxAccount2);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxReceiver, comToken, 100)
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxReceiver, fxAccount1, fxAccount2);
        long xferAmount = (long)fxToken.Params.Circulation;
        await fxToken.TreasuryAccount.Client.TransferTokensAsync(comToken, comToken.TreasuryAccount, fxAccount1, 200, comToken.TreasuryAccount.PrivateKey);
        await fxToken.TreasuryAccount.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, xferAmount, fxToken.TreasuryAccount.PrivateKey);
        var receipt = await fxReceiver.Client.DissociateTokenAsync(comToken, fxReceiver, fxReceiver.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.TreasuryAccount.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, xferAmount, fxAccount1);
        });

        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        await AssertHg.TokenBalanceAsync(comToken, fxAccount1, 200);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, (ulong)xferAmount);

    }

    [Fact(DisplayName = "Royalties: Deleting Fee Receiver Prevents Transfers")]
    public async Task DeletingFeeReceiverPreventsTransfers()
    {
        await using var fxReceiver = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, ctx => ctx.CreateParams.AutoAssociationLimit = 0);
        await using var comToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxReceiver, fxAccount1, fxAccount2);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fxReceiver, comToken, 100)
            };
            fx.Params.GrantKycEndorsement = null;
        }, fxReceiver, fxAccount1, fxAccount2);
        long xferAmount = (long)fxToken.Params.Circulation;
        await fxToken.TreasuryAccount.Client.TransferTokensAsync(comToken, comToken.TreasuryAccount, fxAccount1, 200, comToken.TreasuryAccount.PrivateKey);
        await fxToken.TreasuryAccount.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, xferAmount, fxToken.TreasuryAccount.PrivateKey);

        var receipt = await fxReceiver.Client.DeleteAccountAsync(fxReceiver, comToken.TreasuryAccount, fxReceiver);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.TreasuryAccount.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, xferAmount, fxAccount1);
        });

        Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
        await AssertHg.TokenBalanceAsync(comToken, fxAccount1, 200);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, (ulong)xferAmount);
    }
}