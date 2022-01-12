using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens;

[Collection(nameof(NetworkCredentials))]
public class CreateAssetTests
{
    private readonly NetworkCredentials _network;
    public CreateAssetTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Create Asset: Can Create Asset Definition")]
    public async Task CanCreateAssetDefinition()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);
        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(TokenType.Asset, info.Type);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(0UL, info.Circulation);
        Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);
        Assert.Equal(0, treasury.AssetCount);

        var tokInfo = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, tokInfo.Token);
        Assert.Equal(fxAsset.Params.Symbol, tokInfo.Symbol);
        Assert.Equal(0UL, tokInfo.Balance);
        Assert.Equal(0UL, tokInfo.Decimals);
        Assert.Equal(TokenKycStatus.Granted, tokInfo.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, tokInfo.TradableStatus);
        Assert.False(tokInfo.AutoAssociated);
        Assert.Empty(info.Royalties);

        var record = fxAsset.Record;
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.NotEmpty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Empty(record.TokenTransfers);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        AssertHg.SingleAssociation(fxAsset, fxAsset.TreasuryAccount, record.Associations);
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Create Asset: Can Create Asset Definition with Alias Treasury")]
    public async Task CanCreateAssetDefinitionWithAliasTreasuryDefect()
    {
        // Associating an asset with an account using its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanCreateAssetDefinitionWithAliasTreasury));
        Assert.StartsWith("Unable to create Token, status: InvalidTreasuryAccountForToken", testFailException.Message);

        //[Fact(DisplayName = "Create Asset: Can Create Asset Definition with Alias Treasury")]
        async Task CanCreateAssetDefinitionWithAliasTreasury()
        {
            await using var fxTreasury = await TestAliasAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Metadata = null;
                fx.Params.Treasury = fxTreasury.Alias;
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fxTreasury.PrivateKey);
            });
            Assert.NotNull(fxAsset.Record);
            Assert.NotNull(fxAsset.Record.Token);
            Assert.True(fxAsset.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxTreasury.CreateRecord.Address, info.Treasury);
            Assert.Equal(0UL, info.Circulation);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Royalties);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^

            var treasury = await fxAsset.Client.GetAccountInfoAsync(fxTreasury.CreateRecord.Address);
            Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);
            Assert.Equal(0, treasury.AssetCount);

            var tokInfo = treasury.Tokens[0];
            Assert.Equal(fxAsset.Record.Token, tokInfo.Token);
            Assert.Equal(fxAsset.Params.Symbol, tokInfo.Symbol);
            Assert.Equal(0UL, tokInfo.Balance);
            Assert.Equal(0UL, tokInfo.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokInfo.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokInfo.TradableStatus);
            Assert.False(tokInfo.AutoAssociated);
            Assert.Empty(info.Royalties);

            var record = fxAsset.Record;
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.NotEmpty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Empty(record.TokenTransfers);
            Assert.Empty(record.AssetTransfers);
            Assert.Empty(record.Royalties);
            AssertHg.SingleAssociation(fxAsset, fxAsset.TreasuryAccount, record.Associations);
        }
    }
    [Fact(DisplayName = "Create Asset: Can Create Asset Definition with Fixed Royalty")]
    public async Task CanCreateAssetDefinitionWithFixedRoyalty()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Metadata = null;
            fx.Params.Royalties = new FixedRoyalty[]
            {
                    new FixedRoyalty(fx.TreasuryAccount, Address.None, 1)
            };
        });
        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(TokenType.Asset, info.Type);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(0UL, info.Circulation);
        Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.Single(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var royalty = info.Royalties[0] as FixedRoyalty;
        Assert.NotNull(royalty);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, royalty.Account);
        Assert.Equal(Address.None, royalty.Token);
        Assert.Equal(1, royalty.Amount);
    }
    [Fact(DisplayName = "Create Asset: Can Create (Receipt Version)")]
    public async Task CanCreateAAssetWithReceipt()
    {
        await using var fxTreasury = await TestAccount.CreateAsync(_network);
        await using var fxRenew = await TestAccount.CreateAsync(_network);
        await using var client = _network.NewClient();
        var createParams = new CreateAssetParams
        {
            Name = Generator.Code(50),
            Symbol = Generator.UppercaseAlphaCode(20),
            Ceiling = (long)(Generator.Integer(10, 20) * 100000),
            Treasury = fxTreasury.Record.Address,
            Administrator = fxTreasury.PublicKey,
            GrantKycEndorsement = fxTreasury.PublicKey,
            SuspendEndorsement = fxTreasury.PublicKey,
            PauseEndorsement = fxTreasury.PublicKey,
            ConfiscateEndorsement = fxTreasury.PublicKey,
            SupplyEndorsement = fxTreasury.PublicKey,
            InitializeSuspended = false,
            Expiration = Generator.TruncatedFutureDate(2000, 3000),
            RenewAccount = fxRenew.Record.Address,
            RenewPeriod = TimeSpan.FromDays(90),
            Signatory = new Signatory(fxTreasury.PrivateKey, fxRenew.PrivateKey),
            Memo = Generator.Code(20)
        };
        var receipt = await client.CreateTokenAsync(createParams);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await client.GetTokenInfoAsync(receipt.Token);
        Assert.Equal(receipt.Token, info.Token);
        Assert.Equal(createParams.Symbol, info.Symbol);
        Assert.Equal(createParams.Name, info.Name);
        Assert.Equal(fxTreasury.Record.Address, info.Treasury);
        Assert.Equal(0UL, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(createParams.Administrator, info.Administrator);
        Assert.Equal(createParams.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(createParams.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(createParams.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(createParams.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(createParams.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(createParams.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(createParams.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^
    }
    [Fact(DisplayName = "NETWORK V0.21.0 UNSUPPORTED: Can Create Asset with Alias Treasury")]
    public async Task CanAssociateAssetWithAliasAccountDefect()
    {
        // Creating an asset with a treasury identified by its alias address has not yet been
        // implemented by the network, although it will accept the transaction.
        var testFailException = (await Assert.ThrowsAsync<TransactionException>(CanCreateAnAssetWithAliasTreasuryWithReceipt));
        Assert.StartsWith("Unable to create Token, status: InvalidTreasuryAccountForToken", testFailException.Message);
        //[Fact(DisplayName = "Create Asset: Can Create Asset with Alias Treasury")]
        async Task CanCreateAnAssetWithAliasTreasuryWithReceipt()
        {
            await using var fxTreasury = await TestAliasAccount.CreateAsync(_network);
            await using var fxRenew = await TestAccount.CreateAsync(_network);
            await using var client = _network.NewClient();
            var createParams = new CreateAssetParams
            {
                Name = Generator.Code(50),
                Symbol = Generator.UppercaseAlphaCode(20),
                Ceiling = (long)(Generator.Integer(10, 20) * 100000),
                Treasury = fxTreasury.Alias,
                Administrator = fxTreasury.PublicKey,
                GrantKycEndorsement = fxTreasury.PublicKey,
                SuspendEndorsement = fxTreasury.PublicKey,
                PauseEndorsement = fxTreasury.PublicKey,
                ConfiscateEndorsement = fxTreasury.PublicKey,
                SupplyEndorsement = fxTreasury.PublicKey,
                InitializeSuspended = false,
                Expiration = Generator.TruncatedFutureDate(2000, 3000),
                RenewAccount = fxRenew.Record.Address,
                RenewPeriod = TimeSpan.FromDays(90),
                Signatory = new Signatory(fxTreasury.PrivateKey, fxRenew.PrivateKey),
                Memo = Generator.Code(20)
            };
            var receipt = await client.CreateTokenAsync(createParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await client.GetTokenInfoAsync(receipt.Token);
            Assert.Equal(receipt.Token, info.Token);
            Assert.Equal(createParams.Symbol, info.Symbol);
            Assert.Equal(createParams.Name, info.Name);
            Assert.Equal(fxTreasury.CreateRecord.Address, info.Treasury);
            Assert.Equal(0UL, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(createParams.Administrator, info.Administrator);
            Assert.Equal(createParams.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(createParams.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(createParams.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(createParams.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(createParams.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(createParams.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(createParams.Memo, info.Memo);
            // NETWORK V0.21.0 UNSUPPORTED vvvv
            // NOT IMPLEMENTED YET
            Assert.Empty(info.Ledger.ToArray());
            // NETWORK V0.21.0 UNSUPPORTED ^^^^
        }
    }
    [Fact(DisplayName = "Create Asset: Missing Treasury Address Raises Error")]
    public async Task MissingTreasuryAddressRaisesError()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Treasury = null;
            });
        });
        Assert.Equal("Treasury", aoe.ParamName);
        Assert.StartsWith("The treasury must be specified.", aoe.Message);

        aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Treasury = Address.None;
            });
        });
        Assert.Equal("Treasury", aoe.ParamName);
        Assert.StartsWith("The treasury must be specified.", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: File and Contract Address as Treasury Raises Error")]
    public async Task FileAddressAsTreasuryRaisesError()
    {
        var fxFile = await TestFile.CreateAsync(_network);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Treasury = fxFile.Record.File;
            });
        });
        Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);
        Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Receipt.Status);
        Assert.StartsWith("Unable to create Token, status: InvalidTreasuryAccountForToken", tex.Message);
    }
    [Fact(DisplayName = "Create Asset: Can Set Treasury to Contract Account")]
    public async Task CanSetTreasuryToNodeContractAccount()
    {
        var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Treasury = fxContract.ContractRecord.Contract;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fxContract.PrivateKey);
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);

        var treasury = await fxAsset.Client.GetContractBalancesAsync(fxContract.ContractRecord.Contract);
        var balance = treasury.Tokens.GetValueOrDefault(fxAsset.Record.Token);
        Assert.Equal((ulong)fxAsset.Metadata.Length, balance.Balance);
    }
    [Fact(DisplayName = "Create Asset: Null Administrator Key is Allowed")]
    public async Task NullAdministratorKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.Administrator = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.Administrator);
    }
    [Fact(DisplayName = "Create Asset: Empty Key Administrator Key is Allowed")]
    public async Task EmptyKeyAdministratorKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.Administrator = Endorsement.None;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.Administrator);
    }
    [Fact(DisplayName = "Create Asset: Null GrantKyc Key is Allowed")]
    public async Task NullGrantKycKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.GrantKycEndorsement = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.GrantKycEndorsement);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
    }
    [Fact(DisplayName = "Create Asset: Empty Key GrantKyc Key is Allowed")]
    public async Task EmptyKeyGrantKycKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.GrantKycEndorsement = Endorsement.None;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.GrantKycEndorsement);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
    }
    [Fact(DisplayName = "Create Asset: Null Suspend Key is Allowed")]
    public async Task NullSuspendKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.SuspendEndorsement = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.SuspendEndorsement);
        Assert.Equal(TokenTradableStatus.NotApplicable, info.TradableStatus);
    }
    [Fact(DisplayName = "Create Asset: Empty Key Suspend Key is Allowed")]
    public async Task EmptyKeySuspendKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.SuspendEndorsement = Endorsement.None;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.SuspendEndorsement);
        Assert.Equal(TokenTradableStatus.NotApplicable, info.TradableStatus);
    }
    [Fact(DisplayName = "Create Asset: Null Confiscate Key is Allowed")]
    public async Task NullConfiscateKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.ConfiscateEndorsement = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.ConfiscateEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Empty Confiscate Key is Allowed")]
    public async Task EmptyConfiscateKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.ConfiscateEndorsement = Endorsement.None;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.ConfiscateEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Null Supply Key is Allowed")]
    public async Task NullSupplyKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.SupplyEndorsement = null;
            ctx.Metadata = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.SupplyEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Empty Supply Key is Allowed")]
    public async Task EmptySupplyKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.SupplyEndorsement = Endorsement.None;
            ctx.Metadata = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.SupplyEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Null Royalty Key is Allowed")]
    public async Task NullRoyaltyKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.RoyaltiesEndorsement = null;
            ctx.Metadata = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.RoyaltiesEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Empty Royalty Key is Allowed")]
    public async Task EmptyRoyaltyKeyIsAllowed()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.RoyaltiesEndorsement = Endorsement.None;
            ctx.Metadata = null;
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.RoyaltiesEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Null Symbol is Not Allowed")]
    public async Task NullSymbolIsNotAllowed()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Symbol = null;
            });
        });
        Assert.Equal("Symbol", aoe.ParamName);
        Assert.StartsWith("The token symbol must be specified. (Parameter 'Symbol')", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: Empty Symbol is Not Allowed")]
    public async Task EmptySymbolIsNotAllowed()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Symbol = string.Empty;
            });
        });
        Assert.Equal("Symbol", aoe.ParamName);
        Assert.StartsWith("The token symbol must be specified. (Parameter 'Symbol')", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: Symbol Does Numbers")]
    public async Task SymbolDoesAllowNumbers()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.Symbol = "123" + Generator.UppercaseAlphaCode(20) + "456";
        });
        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var assets = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, assets.Token);
        Assert.Equal(fxAsset.Params.Symbol, assets.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, assets.Balance);
        Assert.Equal(0U, assets.Decimals);
        Assert.Equal(TokenKycStatus.Granted, assets.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, assets.TradableStatus);
        Assert.False(assets.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Duplicate Symbols Are Allowed")]
    public async Task TaskDuplicateSymbolsAreAllowed()
    {
        await using var fxTaken = await TestAsset.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Symbol = fxTaken.Params.Symbol;
        });

        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var asset = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, asset.Token);
        Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, asset.Balance);
        Assert.Equal(0U, asset.Decimals);
        Assert.Equal(TokenKycStatus.Granted, asset.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
        Assert.False(asset.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Null Name is Not Allowed")]
    public async Task NullNameIsNotAllowed()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Name = null;
            });
        });
        Assert.Equal("Name", aoe.ParamName);
        Assert.StartsWith("The name cannot be null or empty. (Parameter 'Name')", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: Empty Name is Not Allowed")]
    public async Task EmptyNameIsNotAllowed()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await using var fx = await TestAsset.CreateAsync(_network, ctx =>
            {
                ctx.Params.Name = string.Empty;
            });
        });
        Assert.Equal("Name", aoe.ParamName);
        Assert.StartsWith("The name cannot be null or empty. (Parameter 'Name')", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: Name Does Allow Numbers and Spaces")]
    public async Task NameDoesAllowNumbersAndSpaces()
    {
        await using var fx = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.Name = Generator.UppercaseAlphaCode(20) + " 123\r\n\t?";
        });
        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Equal(fx.Params.Name, info.Name);
    }
    [Fact(DisplayName = "Create Asset: Duplicate Symbols Are Allowed")]
    public async Task TaskDuplicateNamesAreAllowed()
    {
        await using var fxTaken = await TestAsset.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Name = fxTaken.Params.Name;
        });

        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var asset = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, asset.Token);
        Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, asset.Balance);
        Assert.Equal(0U, asset.Decimals);
        Assert.Equal(TokenKycStatus.Granted, asset.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
        Assert.False(asset.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Initialize Supended Can Be True")]
    public async Task InitializeSupendedCanBeFalse()
    {
        await using var fx = await TestAsset.CreateAsync(_network, ctx =>
        {
            ctx.Params.InitializeSuspended = true;
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Equal(TokenTradableStatus.Suspended, info.TradableStatus);
    }
    [Fact(DisplayName = "Create Asset: Initialize Is False By Default")]
    public async Task InitializeIsFalseByDefault()
    {
        await using var fx = await TestAsset.CreateAsync(_network);

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
    }
    [Fact(DisplayName = "Create Asset: AutoRenew Account is not Required")]
    public async Task AutoRenewAccountIsNotRequired()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.RenewAccount = null;
            fx.Params.RenewPeriod = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.TreasuryAccount.PrivateKey);
        });

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Null(info.RenewAccount);
    }
    [Fact(DisplayName = "Create Asset: Missing Admin Signature Raises Error")]
    public async Task MissingAdminSignatureRaisesError()
    {
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to create Token, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Create Asset: Missing Grant Admin Signature Is Allowed")]
    public async Task MissingGrantAdminSignatureIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.NotNull(info.GrantKycEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Missing Suspend Admin Signature Is Allowed")]
    public async Task MissingSuspendAdminSignatureIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.NotNull(info.SuspendEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Missing Confiscate Admin Signature Is Allowed")]
    public async Task MissingConfiscateAdminSignatureIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.NotNull(info.ConfiscateEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Missing Supply Admin Signature Is Allowed")]
    public async Task MissingSupplyAdminSignatureIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.NotNull(info.SupplyEndorsement);
    }
    [Fact(DisplayName = "Create Asset: Null Memo is Allowed")]
    public async Task NullMemoIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Memo = null;
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Empty(info.Memo);
    }
    [Fact(DisplayName = "Create Asset: Empty Memo is Allowed")]
    public async Task EmptyMemoIsAllowed()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Memo = string.Empty;
        });

        var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
        Assert.Empty(info.Memo);
    }
    [Fact(DisplayName = "Create Asset: Missing Renew Account Signature Raises Error")]
    public async Task CreateAssetMissingRenewAccountSignatureRaisesError()
    {
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.TreasuryAccount.PrivateKey);
            });
        });
        Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
        Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
        Assert.StartsWith("Unable to create Token, status: InvalidSignature", tex.Message);
    }
    [Fact(DisplayName = "Create Asset: Expiration time in Past Raises Error")]
    public async Task ExpirationTimeInPastRaisesError()
    {
        var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
        {
            await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Expiration = DateTime.UtcNow.AddDays(-5);
            });
        });
        Assert.Equal("Expiration", aoe.ParamName);
        Assert.StartsWith("The expiration time must be in the future.", aoe.Message);
    }
    [Fact(DisplayName = "Create Asset: Only Admin, Treasury and Renew Account Keys are Requied")]
    public async Task OnlyAdminTreasuryAndRenewAccountKeysAreRequied()
    {
        await using var fx = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.Success, fx.Record.Status);
    }
    [Fact(DisplayName = "Create Asset: Can Create With ReUsed Symbol From Deleted Asset")]
    public async Task CanCreateWithReUsedSymbolFromDeletedAsset()
    {
        await using var fxTaken = await TestAsset.CreateAsync(_network);
        await fxTaken.Client.DeleteTokenAsync(fxTaken, fxTaken.AdminPrivateKey);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Symbol = fxTaken.Params.Symbol;
        });

        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var asset = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, asset.Token);
        Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, asset.Balance);
        Assert.Equal(0U, asset.Decimals);
        Assert.Equal(TokenKycStatus.Granted, asset.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
        Assert.False(asset.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Can Create With ReUsed Name From Deleted Asset")]
    public async Task CanCreateWithReUsedNameFromDeletedAsset()
    {
        await using var fxTaken = await TestAsset.CreateAsync(_network);
        await fxTaken.Client.DeleteTokenAsync(fxTaken, fxTaken.AdminPrivateKey);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Name = fxTaken.Params.Name;
        });

        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var asset = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, asset.Token);
        Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, asset.Balance);
        Assert.Equal(0U, asset.Decimals);
        Assert.Equal(TokenKycStatus.Granted, asset.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
        Assert.False(asset.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Can Create with Contract as Treasury")]
    public async Task CanCreateWithContractAsTreasury()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.Treasury = fxContract.ContractRecord.Contract;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fxContract.PrivateKey, fx.RenewAccount.PrivateKey);
        });
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);
        Assert.Equal(fxAsset.Params.Memo, info.Memo);
        // NETWORK V0.21.0 UNSUPPORTED vvvv
        // NOT IMPLEMENTED YET
        Assert.Empty(info.Ledger.ToArray());
        // NETWORK V0.21.0 UNSUPPORTED ^^^^

        var treasury = await fxAsset.Client.GetContractBalancesAsync(fxContract);
        Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, treasury.Crypto);
        Assert.Single(treasury.Tokens);
        Assert.Equal((ulong)fxAsset.Metadata.Length, treasury.Tokens[fxAsset].Balance);
        Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetContractTokenBalanceAsync(fxContract, fxAsset));
    }
    [Fact(DisplayName = "Create Asset: Can Create without Renewal Information")]
    public async Task CanCreateWithoutRenewalInformation()
    {
        await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
        {
            fx.Params.RenewAccount = null;
            fx.Params.RenewPeriod = default;
        });
        Assert.NotNull(fxAsset.Record);
        Assert.NotNull(fxAsset.Record.Token);
        Assert.True(fxAsset.Record.Token.AccountNum > 0);
        Assert.Equal(ResponseCode.Success, fxAsset.Record.Status);

        var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
        Assert.Equal(fxAsset.Record.Token, info.Token);
        Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
        Assert.Equal(fxAsset.Params.Name, info.Name);
        Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
        Assert.Equal(0U, info.Decimals);
        Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
        Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
        Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Null(info.RenewPeriod);
        Assert.Null(info.RenewAccount);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        Assert.False(info.Deleted);

        var treasury = await fxAsset.Client.GetAccountInfoAsync(fxAsset.TreasuryAccount.Record.Address);
        Assert.Equal(fxAsset.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
        Assert.Single(treasury.Tokens);

        var asset = treasury.Tokens[0];
        Assert.Equal(fxAsset.Record.Token, asset.Token);
        Assert.Equal(fxAsset.Params.Symbol, asset.Symbol);
        Assert.Equal((ulong)fxAsset.Metadata.Length, asset.Balance);
        Assert.Equal(0U, asset.Decimals);
        Assert.Equal(TokenKycStatus.Granted, asset.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, asset.TradableStatus);
        Assert.False(asset.AutoAssociated);
    }
    [Fact(DisplayName = "Create Asset: Can Not Schedule a Create Asset")]
    public async Task CanNotScheduleACreateAsset()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(
                    fx.Params.Signatory,
                    new PendingParams
                    {
                        PendingPayer = fxPayer,
                    });
            });
        });
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
        Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Receipt.Status);
        Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
    }
}