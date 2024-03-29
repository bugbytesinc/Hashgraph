﻿namespace Hashgraph.Test.Token;

[Collection(nameof(NetworkCredentials))]
public class UpdateTokenTests
{
    private readonly NetworkCredentials _network;
    public UpdateTokenTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Update Token: Can Update Token")]
    public async Task CanUpdateToken()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxTemplate = await TestToken.CreateAsync(_network);

        await fxTemplate.TreasuryAccount.Client.AssociateTokenAsync(fxToken, fxTemplate.TreasuryAccount, fxTemplate.TreasuryAccount);

        var newSymbol = Generator.Code(20);
        var newName = Generator.String(20, 50);
        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxTemplate.Params.Treasury,
            Administrator = fxTemplate.Params.Administrator,
            GrantKycEndorsement = fxTemplate.Params.GrantKycEndorsement,
            SuspendEndorsement = fxTemplate.Params.SuspendEndorsement,
            ConfiscateEndorsement = fxTemplate.Params.ConfiscateEndorsement,
            SupplyEndorsement = fxTemplate.Params.SupplyEndorsement,
            Symbol = newSymbol,
            Name = newName,
            Expiration = DateTime.UtcNow.AddDays(180),
            RenewPeriod = fxTemplate.Params.RenewPeriod,
            RenewAccount = fxTemplate.RenewAccount,
            Signatory = new Signatory(fxToken.Params.Signatory, fxTemplate.Params.Signatory),
            Memo = fxTemplate.Params.Memo
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(newSymbol, info.Symbol);
        Assert.Equal(newName, info.Name);
        Assert.Equal(fxTemplate.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
        Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.Equal(fxTemplate.Params.Memo, info.Memo);
        Assert.False(info.Deleted);
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Can Update Token and get Record")]
    public async Task CanUpdateTokenAndGetRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxTemplate = await TestToken.CreateAsync(_network);

        // It looks like changing the treasury requires the receiving account to be
        // associated first, since it still has to sign the update transaction anyway,
        // this seems unecessary.
        await fxTemplate.TreasuryAccount.Client.AssociateTokenAsync(fxToken, fxTemplate.TreasuryAccount, fxTemplate.TreasuryAccount);

        var newSymbol = Generator.Code(20);
        var newName = Generator.String(20, 50);
        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxTemplate.Params.Treasury,
            Administrator = fxTemplate.Params.Administrator,
            GrantKycEndorsement = fxTemplate.Params.GrantKycEndorsement,
            SuspendEndorsement = fxTemplate.Params.SuspendEndorsement,
            ConfiscateEndorsement = fxTemplate.Params.ConfiscateEndorsement,
            SupplyEndorsement = fxTemplate.Params.SupplyEndorsement,
            RoyaltiesEndorsement = fxTemplate.Params.RoyaltiesEndorsement,
            Symbol = newSymbol,
            Name = newName,
            Expiration = DateTime.UtcNow.AddDays(180),
            RenewPeriod = fxTemplate.Params.RenewPeriod,
            RenewAccount = fxTemplate.RenewAccount,
            Signatory = new Signatory(fxToken.Params.Signatory, fxTemplate.Params.Signatory),
            Memo = fxTemplate.Params.Memo
        };

        var record = await fxToken.Client.UpdateTokenWithRecordAsync(updateParams);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(newSymbol, info.Symbol);
        Assert.Equal(newName, info.Name);
        Assert.Equal(fxTemplate.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
        Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxTemplate.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxTemplate.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Empty Update Parameters Raises Error")]
    public async Task EmptyUpdateParametersRaisesError()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Signatory = fxToken.Params.Signatory
        };
        var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal("updateParameters", ae.ParamName);
        Assert.StartsWith("The Topic Updates contain no update properties, it is blank.", ae.Message);
    }
    [Fact(DisplayName = "Update Token: Can Update Treasury")]
    public async Task CanUpdateTreasury()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxAccount.Record.Address,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount.PrivateKey)
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxAccount.Record.Address, info.Treasury);
    }
    [Fact(DisplayName = "Update Token: Can Update Admin Endorsment")]
    public async Task CanUpdateAdminEndorsement()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Administrator = newPublicKey,
            Signatory = new Signatory(fxToken.AdminPrivateKey, newPrivateKey)
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(updateParams.Administrator, info.Administrator);
    }
    [Fact(DisplayName = "Update Token: Can Update Grant KYC Endorsement")]
    public async Task CanUpdateGrantKycEndorsement()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            GrantKycEndorsement = newPublicKey,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(updateParams.GrantKycEndorsement, info.GrantKycEndorsement);
    }
    [Fact(DisplayName = "Update Token: Can Update Suspend Endorsement")]
    public async Task CanUpdateSuspendEndorsement()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            SuspendEndorsement = newPublicKey,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(updateParams.SuspendEndorsement, info.SuspendEndorsement);
    }
    [Fact(DisplayName = "Update Token: Can Update Confiscate Endorsement")]
    public async Task CanUpdateConfiscateEndorsement()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            ConfiscateEndorsement = newPublicKey,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(updateParams.ConfiscateEndorsement, info.ConfiscateEndorsement);
    }
    [Fact(DisplayName = "Update Token: Can Update Supply Endorsement")]
    public async Task CanUpdateSupplyEndorsement()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            SupplyEndorsement = newPublicKey,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(updateParams.SupplyEndorsement, info.SupplyEndorsement);
    }
    [Fact(DisplayName = "Update Token: Can Update Symbol")]
    public async Task CanUpdateSymbol()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newSymbol = Generator.Code(20);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Symbol = newSymbol,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newSymbol, info.Symbol);
    }
    [Fact(DisplayName = "Update Token: Can Update Name")]
    public async Task CanUpdateName()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newName = Generator.String(30, 50);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Name = newName,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newName, info.Name);
    }
    [Fact(DisplayName = "Update Token: Can Update Memo")]
    public async Task CanUpdateMemo()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newMemo = Generator.Memo(30, 50);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Memo = newMemo,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newMemo, info.Memo);
    }
    [Fact(DisplayName = "Update Token: Can Update Memo to Empty")]
    public async Task CanUpdateMemoToEmpty()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Memo = string.Empty,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Empty(info.Memo);
    }
    [Fact(DisplayName = "Update Token: Can Update Expiration")]
    public async Task CanUpdateExpiration()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var newExpiration = Generator.TruncateToSeconds(DateTime.UtcNow.AddDays(365));

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Expiration = newExpiration,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newExpiration, info.Expiration);
    }
    [Fact(DisplayName = "Update Token: Can Update Renew Period")]
    public async Task CanUpdateRenewPeriod()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newRenwew = TimeSpan.FromDays(90) + TimeSpan.FromMinutes(10);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            RenewPeriod = newRenwew,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newRenwew, info.RenewPeriod);
    }
    [Fact(DisplayName = "Update Token: Can Update Renew Account")]
    public async Task CanUpdateRenewAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newRenwew = TimeSpan.FromDays(89);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            RenewAccount = fxAccount.Record.Address,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount.PrivateKey)
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxAccount.Record.Address, info.RenewAccount);
    }
    [Fact(DisplayName = "Update Token: Any Account With Admin Key Can Update")]
    public async Task AnyAccountWithAdminKeyCanUpdate()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newName = Generator.String(30, 50);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Name = newName,
            Signatory = fxToken.AdminPrivateKey
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams, ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(newName, info.Name);
    }
    [Fact(DisplayName = "Update Token: Updates Require Admin Key")]
    public async Task UpdatesRequireAdminKey()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Name = Generator.String(30, 50)
        };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Updating To Used Symbol Is Allowed")]
    public async Task UpdatingToUsedSymbolIsAllowed()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxOther = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Symbol = fxOther.Params.Symbol,
            Signatory = fxToken.AdminPrivateKey
        };
        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxOther.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Updating To Used Name Is Allowed")]
    public async Task UpdatingToUsedNameIsAllowed()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);
        await using var fxOther = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Name = fxOther.Params.Name,
            Signatory = fxToken.AdminPrivateKey
        };

        await fxToken.Client.UpdateTokenAsync(updateParams);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxOther.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Updating To Empty Treasury Address Raises Error")]
    public async Task UpdatingToEmptyTreasuryRaisesError()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = Address.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.AccountIdDoesNotExist, tex.Status);
        Assert.Equal(ResponseCode.AccountIdDoesNotExist, tex.Receipt.Status);
        Assert.StartsWith("Unable to update Token, status: AccountIdDoesNotExist", tex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Make Token Immutable")]
    public async Task CannotMakeTokenImmutable()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Administrator = Endorsement.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidAdminKey, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidAdminKey", pex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Remove Grant KYC Endorsement")]
    public async Task CannotRemoveGrantKYCEndorsement()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            GrantKycEndorsement = Endorsement.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidKycKey, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidKycKey", pex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Remove Suspend Endorsement")]
    public async Task CannotRemoveSuspendEndorsement()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            SuspendEndorsement = Endorsement.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidFreezeKey, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidFreezeKey", pex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Remove Confiscate Endorsement")]
    public async Task CannotRemoveConfiscateEndorsement()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            ConfiscateEndorsement = Endorsement.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidWipeKey, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidWipeKey", pex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Remove Supply Endorsement")]
    public async Task CannotRemoveSupplyEndorsement()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            SupplyEndorsement = Endorsement.None,
            Signatory = fxToken.AdminPrivateKey
        };

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidSupplyKey, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidSupplyKey", pex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Cannot Update Imutable Token")]
    public async Task CannotUpdateImutableToken()
    {
        var (newPublicKey, newPrivateKey) = Generator.KeyPair();
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.Administrator = null);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            SupplyEndorsement = newPublicKey,
            Signatory = fxToken.AdminPrivateKey
        };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.TokenIsImmutable, tex.Status);
        Assert.Equal(ResponseCode.TokenIsImmutable, tex.Receipt.Status);
        Assert.StartsWith("Unable to update Token, status: TokenIsImmutable", tex.Message);
    }
    [Fact(DisplayName = "Update Token: Updating the Treasury Without Signing Raises Error")]
    public async Task UpdatingTheTreasuryWithoutSigningRaisesError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxAccount.Record.Address,
            Signatory = fxToken.AdminPrivateKey
        };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);

        Assert.Equal((long)fxToken.Params.Circulation, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
    }

    [Fact(DisplayName = "Update Token: Updating the Treasury Without Signing Without Admin Key Raises Error")]
    public async Task UpdatingTheTreasuryWithoutSigningWithoutAdminKeyRaisesError()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxAccount.Record.Address,
            Signatory = fxAccount.PrivateKey
        };

        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(updateParams);
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.Params.Name, info.Name);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
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
        AssertHg.Equal(_network.Ledger, info.Ledger);

        Assert.Equal((long)fxToken.Params.Circulation, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal(0, await fxAccount.GetTokenBalanceAsync(fxToken));
    }
    [Fact(DisplayName = "Update Token: Can Update Treasury to Contract")]
    public async Task CanUpdateTreasuryToContract()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network);

        // Note: Contract did not need to sign.
        await fxContract.Client.AssociateTokenAsync(fxToken, fxContract, fxContract.PrivateKey);

        var updateParams = new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxContract.ContractRecord.Contract,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxContract.PrivateKey)
        };

        var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);

        Assert.Equal((long)fxToken.Params.Circulation, await fxContract.GetTokenBalanceAsync(fxToken));
    }
    [Fact(DisplayName = "Update Token: Can Delete an Auto Renew Account while Used by Token")]
    public async Task RemovingAnAutoRenewAccountIsNotAllowed()
    {
        await using var fxToken = await TestToken.CreateAsync(_network);

        var receipt = await fxToken.Client.DeleteAccountAsync(fxToken.RenewAccount, _network.Payer, fxToken.RenewAccount);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RenewAccount, info.RenewAccount);
        Assert.Equal(fxToken.Params.RenewPeriod, info.RenewPeriod);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);
    }
    [Fact(DisplayName = "Update Token: Can Not Change Treasury to Unassociated Account")]
    public async Task CanChangeTreasuryToUnassociatedAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 0);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.ConfiscateEndorsement = null;
            fx.Params.SuspendEndorsement = null;
            fx.Params.GrantKycEndorsement = null;
        });
        var totalCirculation = fxToken.Params.Circulation;

        Assert.Null(await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)totalCirculation, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));

        // Returns A Failure
        var tex = await Assert.ThrowsAnyAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
            {
                Token = fxToken.Record.Token,
                Treasury = fxAccount.Record.Address,
                Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount.PrivateKey)
            });
        });

        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Status);
        Assert.Equal(ResponseCode.NoRemainingAutomaticAssociations, tex.Receipt.Status);

        // Confirm it did not change the Treasury Account
        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
    }
    [Fact(DisplayName = "Update Token: Can Change Treasury to AutoAssociated Account")]
    public async Task CanChangeTreasuryToAutoAssociatedAccount()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.AutoAssociationLimit = 1);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.ConfiscateEndorsement = null;
            fx.Params.SuspendEndorsement = null;
            fx.Params.GrantKycEndorsement = null;
        });
        var totalCirculation = fxToken.Params.Circulation;

        Assert.Null(await fxAccount.GetTokenBalanceAsync(fxToken));
        Assert.Equal((long)totalCirculation, await fxToken.TreasuryAccount.GetTokenBalanceAsync(fxToken));

        await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
        {
            Token = fxToken.Record.Token,
            Treasury = fxAccount.Record.Address,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount.PrivateKey)
        });

        // Confirm it did change the Treasury Account
        var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
        Assert.Equal(fxAccount.Record.Address, info.Treasury);
    }
    [Fact(DisplayName = "Update Token: Can Not Schedule Update Token")]
    public async Task CanNotScheduleUpdateToken()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxToken = await TestToken.CreateAsync(_network);
        var newSymbol = Generator.Code(20);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
            {
                Token = fxToken.Record.Token,
                Symbol = newSymbol,
                Signatory = new Signatory(
                    fxToken.AdminPrivateKey,
                    new PendingParams
                    {
                        PendingPayer = fxPayer
                    })
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
    [Fact(DisplayName = "Update Token: Can Update Token Fixed Royalty")]
    public async Task CanUpdateTokenFixedRoyalty()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fx.TreasuryAccount, Address.None, 10)
            };
        });

        var royalties = new FixedRoyalty[]
        {
               new FixedRoyalty(fxToken.TreasuryAccount, Address.None, 1)
        };

        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, royalties, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        var royalty = info.Royalties[0] as FixedRoyalty;
        Assert.NotNull(royalty);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, royalty.Account);
        Assert.Equal(Address.None, royalty.Token);
        Assert.Equal(1, royalty.Amount);
    }
    [Fact(DisplayName = "Update Token: Can Update Token Fixed Royalty And Get Record")]
    public async Task CanUpdateTokenFixedRoyaltyAndGetRecord()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fx.TreasuryAccount, Address.None, 10)
            };
        });

        var royalties = new FixedRoyalty[]
        {
               new FixedRoyalty(fxToken.TreasuryAccount, Address.None, 1)
        };

        var record = await fxToken.Client.UpdateRoyaltiesWithRecordAsync(fxToken, royalties, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        var royalty = info.Royalties[0] as FixedRoyalty;
        Assert.NotNull(royalty);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, royalty.Account);
        Assert.Equal(Address.None, royalty.Token);
        Assert.Equal(1, royalty.Amount);
    }
    [Fact(DisplayName = "Update Token: Can Update Token Fixed Royalty And Get Record with Signatory in Context")]
    public async Task CanUpdateTokenFixedRoyaltyAndGetRecordWithSignatoryInContext()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fx.TreasuryAccount, Address.None, 10)
            };
        });

        var royalties = new FixedRoyalty[]
        {
               new FixedRoyalty(fxToken.TreasuryAccount, Address.None, 1)
        };

        var record = await fxToken.Client.UpdateRoyaltiesWithRecordAsync(fxToken, royalties, ctx => ctx.Signatory = new Signatory(ctx.Signatory, fxToken.RoyaltiesPrivateKey));
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        var royalty = info.Royalties[0] as FixedRoyalty;
        Assert.NotNull(royalty);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, royalty.Account);
        Assert.Equal(Address.None, royalty.Token);
        Assert.Equal(1, royalty.Amount);
    }
    [Fact(DisplayName = "Update Token: Can Update Token Fractional Royalty")]
    public async Task CanUpdateTokenFractionalRoyalty()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new TokenRoyalty[]
            {
                    new TokenRoyalty(fx.TreasuryAccount, 1, 10, 1, 10)
            };
        });

        var royalties = new TokenRoyalty[]
        {
                new TokenRoyalty(fxToken.TreasuryAccount, 1, 2, 1, 100)
        };

        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, royalties, fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Single(info.Royalties);

        var royalty = info.Royalties[0] as TokenRoyalty;
        Assert.NotNull(royalty);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, royalty.Account);
        Assert.Equal(1, royalty.Numerator);
        Assert.Equal(2, royalty.Denominator);
        Assert.Equal(1, royalty.Minimum);
        Assert.Equal(100, royalty.Maximum);
        Assert.False(royalty.AssessAsSurcharge);
    }
    [Fact(DisplayName = "Update Token: Can Clear Royalty Tables")]
    public async Task CanClearAndFreezeRoyaltyTables()
    {
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fx.TreasuryAccount, Address.None, 1)
            };
        });

        var receipt = await fxToken.Client.UpdateRoyaltiesAsync(fxToken, Array.Empty<IRoyalty>(), fxToken.RoyaltiesPrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Empty(info.Royalties);
    }
    [Fact(DisplayName = "Create Topic: Can Renew Imutable Topic")]
    public async Task CanRenewImutableTopic()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 10_00_000_000);

        await using var client = fxAccount.Client.Clone(ctx =>
        {
            ctx.Payer = fxAccount;
            ctx.Signatory = fxAccount;
        });

        var receipt = await client.CreateTopicAsync(new CreateTopicParams
        {
            Memo = "TEST",
        });
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxAccount.Client.GetTopicInfoAsync(receipt.Topic);
        Assert.Equal("TEST", info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.True(info.Expiration > ConsensusTimeStamp.MinValue);
        Assert.Null(info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Null(info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);

        var newExpiration = info.Expiration.AddMinutes(60 * 24 * 30);
        var renew = await client.UpdateTopicAsync(new UpdateTopicParams()
        {
            Topic = receipt.Topic,
            Expiration = newExpiration
        });
        Assert.Equal(ResponseCode.Success, renew.Status);

        info = await fxAccount.Client.GetTopicInfoAsync(receipt.Topic);
        Assert.Equal("TEST", info.Memo);
        Assert.NotEmpty(info.RunningHash.ToArray());
        Assert.Equal(0UL, info.SequenceNumber);
        Assert.Equal(newExpiration, info.Expiration);
        Assert.Null(info.Administrator);
        Assert.Null(info.Participant);
        Assert.True(info.AutoRenewPeriod > TimeSpan.MinValue);
        Assert.Null(info.RenewAccount);
        AssertHg.NotEmpty(info.Ledger);

    }
}