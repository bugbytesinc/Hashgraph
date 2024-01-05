namespace Hashgraph.Test.AssetTokens;

[Collection(nameof(NetworkCredentials))]
public class DissociateAssetTests
{
    private readonly NetworkCredentials _network;
    public DissociateAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Account")]
    public async Task CanDissociateAssetFromAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var receipt = await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Dissociate Assets: Can Dissociate asset from Alias Account")]
    public async Task CanDissociateAssetFromAliasAccountDefect()
    {
        // DisAssociating an asset with an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanDissociateAssetFromAliasAccount));
        Assert.StartsWith("Unable to Dissociate Token from Account, status: InvalidAccountId", testFailException.Message);

        //[Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Alias Account")]
        async Task CanDissociateAssetFromAliasAccount()
        {
            await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
            var receipt = await fxAsset.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount, fxAccount.PrivateKey);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(0, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
            Assert.False(association.AutoAssociated);

            receipt = await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Alias, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
        }
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Account and get Record")]
    public async Task CanDissociateAssetFromAccountAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var record = await fxAccount.Client.DissociateTokenWithRecordAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Account (No Extra Signatory)")]
    public async Task CanDissociateAssetFromAccountNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var receipt = await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Account and get Record (No Extra Signatory)")]
    public async Task CanDissociateAssetFromAccountAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var record = await fxAccount.Client.DissociateTokenWithRecordAsync(fxAsset.Record.Token, fxAccount.Record.Address, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(fxAccount.Record.Address, record.Id.Address);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate Multpile Assets with Account")]
    public async Task CanDissociateMultipleAssetsWithAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, null, fxAccount);

        var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);

        var receipt = await fxAccount.Client.DissociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate Multiple Assets with Account and get Record")]
    public async Task CanDissociateMultipleAssetsWithAccountAndGetRecord()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, null, fxAccount);

        var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);

        var record = await fxAccount.Client.DissociateTokensWithRecordAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);



        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate Multiple Asset with Account (No Extra Signatory)")]
    public async Task CanDissociateMultipleAssetsWithAccountNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, null, fxAccount);

        var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);

        var receipt = await fxAccount.Client.DissociateTokensAsync(assets, fxAccount.Record.Address, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate Multiple Asset with Account and get Record (No Extra Signatory)")]
    public async Task CanDissociateMultipleAssetsWithAccountAndGetRecordNoExtraSignatory()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network, null, fxAccount);

        var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

        await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);

        var record = await fxAccount.Client.DissociateTokensWithRecordAsync(assets, fxAccount.Record.Address, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(fxAccount.Record.Address, record.Id.Address);

        await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
        await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: No Asset Balance Record Exists When Dissociated")]
    public async Task NoAssetBalanceRecordExistsWhenDissociated()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var receipt = await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
    }
    [Fact(DisplayName = "Dissociate Assets: Dissociation Requires Signing by Target Account")]
    public async Task DissociationRequiresSigningByTargetAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to Dissociate Token from Account, status: InvalidSignature", tex.Message);

        association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);
    }
    [Fact(DisplayName = "Dissociate Assets: Dissociation Requires Asset Account")]
    public async Task DissociationRequiresAssetAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(Address.None, fxAccount.Record.Address);
        });
        Assert.Equal("token", ane.ParamName);
        Assert.StartsWith("Token is missing. Please check that it is not null or empty.", ane.Message);

        ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(null, fxAccount.Record.Address);
        });
        Assert.Equal("token", ane.ParamName);
        Assert.StartsWith("Token is missing. Please check that it is not null or empty.", ane.Message);

        ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fxAccount.Client.DissociateTokensAsync(null, fxAccount.Record.Address);
        });
        Assert.Equal("tokens", ane.ParamName);
        Assert.StartsWith("The list of tokens cannot be null.", ane.Message);

        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await fxAccount.Client.DissociateTokensAsync(new Address[] { null }, fxAccount.Record.Address);
        });
        Assert.Equal("tokens", aoe.ParamName);
        Assert.StartsWith("The list of tokens cannot contain an empty or null address.", aoe.Message);
    }
    [Fact(DisplayName = "Dissociate Assets: Dissociation Requires Account")]
    public async Task DissociationRequiresAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, null);
        });
        Assert.Equal("account", ane.ParamName);
        Assert.StartsWith("Account Address/Alias is missing. Please check that it is not null.", ane.Message);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, Address.None);
        });
        Assert.Equal(ResponseCode.InvalidAccountId, tex.Status);
        Assert.Equal(ResponseCode.InvalidAccountId, tex.Receipt.Status);
        Assert.StartsWith("Unable to Dissociate Token from Account, status: InvalidAccountId", tex.Message);
    }
    [Fact(DisplayName = "Dissociate Assets: Dissociating with Deleted Account Raises Error")]
    public async Task DissociatingWithDeletedAccountRaisesError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        await fxAccount.Client.DeleteAccountAsync(fxAccount, _network.Payer, fxAccount);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
        Assert.Equal(ResponseCode.AccountDeleted, tex.Receipt.Status);
        Assert.StartsWith("Unable to Dissociate Token from Account, status: AccountDelete", tex.Message);

    }
    [Fact(DisplayName = "Dissociate Assets: Dissociating with Duplicate Asset Raises Error")]
    public async Task DissociatingWithDuplicateAccountRaisesError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            var assets = new Address[] { fxAsset.Record.Token, fxAsset.Record.Token };
            await fxAsset.Client.DissociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.TokenIdRepeatedInTokenList, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: TokenIdRepeatedInTokenList", pex.Message);
    }
    [Fact(DisplayName = "Dissociate Assets: Dissociate with Dissociated Asset Raises Error")]
    public async Task DissociateWithDissociatedAssetRaisesError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
        await using var fxAsset2 = await TestAsset.CreateAsync(_network);

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };
            await fxAccount.Client.DissociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Receipt.Status);
        Assert.StartsWith("Unable to Dissociate Token from Account, status: TokenNotAssociatedToAccount", tex.Message);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Dissociate asset from Contract Consent")]
    public async Task CanDissociateAssetFromContract()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);

        var receipt = await fxContract.Client.AssociateTokenAsync(fxAsset.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxAccount.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
        Assert.NotNull(info);

        var balances = await fxContract.GetTokenBalancesAsync();
        Assert.Single(balances);

        var association = balances[0];
        Assert.NotNull(association);
        Assert.Equal(fxAsset.Record.Token, association.Token);
        Assert.Equal(0, association.Balance);
        Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.FreezeStatus);
        Assert.False(association.AutoAssociated);

        receipt = await fxContract.Client.DissociateTokenAsync(fxAsset.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        info = await fxAccount.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
        Assert.NotNull(info);

        Assert.Empty(await fxContract.GetTokenBalancesAsync());

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAsset.Client.TransferAssetAsync(new Asset(fxAsset, 1), fxAsset.TreasuryAccount.Record.Address, fxContract.ContractRecord.Contract, fxAsset.TreasuryAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
        Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", tex.Message);

        Assert.Empty(await fxContract.GetTokenBalancesAsync());
        Assert.Equal((long)fxAsset.Metadata.Length, await fxAsset.TreasuryAccount.GetTokenBalanceAsync(fxAsset));
    }
    [Fact(DisplayName = "Asset Delete: Can Not Delete Account Having Asset Balance")]
    public async Task CanNOtDeleteAccountHavingAssetBalance()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);

        var asset = new Asset(fxAsset, 1);

        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length);

        var receipt = await fxAccount1.Client.TransferAssetAsync(asset, fxAsset.TreasuryAccount, fxAccount1, fxAsset.TreasuryAccount);

        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

        // Can't delete the account because it has assets associated with it.
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount1.Client.DeleteAccountAsync(fxAccount1, fxAccount2, fxAccount1.PrivateKey);
        });
        Assert.Equal(ResponseCode.TransactionRequiresZeroTokenBalances, tex.Status);
        Assert.Equal(ResponseCode.TransactionRequiresZeroTokenBalances, tex.Receipt.Status);
        Assert.StartsWith("Unable to delete account, status: TransactionRequiresZeroTokenBalances", tex.Message);

        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount1, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 0);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);

        await fxAccount1.Client.TransferAssetAsync(asset, fxAccount1, fxAccount2, fxAccount1);
        receipt = await fxAccount1.Client.DeleteAccountAsync(fxAccount1, fxAccount2, fxAccount1.PrivateKey);

        await AssertHg.AssetBalanceAsync(fxAsset, fxAccount2, 1);
        await AssertHg.AssetBalanceAsync(fxAsset, fxAsset.TreasuryAccount, (ulong)fxAsset.Metadata.Length - 1);
    }
    [Fact(DisplayName = "Dissociate Assets: Can Not Schedule Dissociate asset from Account")]
    public async Task CanNotScheduleDissociateAssetFromAccount()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

        await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxAccount.Client.DissociateTokenAsync(
                fxAsset.Record.Token,
                fxAccount.Record.Address,
                new Signatory(
                    fxAccount.PrivateKey,
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