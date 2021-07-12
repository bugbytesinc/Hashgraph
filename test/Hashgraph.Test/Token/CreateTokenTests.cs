using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class CreateTokenTests
    {
        private readonly NetworkCredentials _network;
        public CreateTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Create Token: Can Create")]
        public async Task CanCreateAToken()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Can Create (Receipt Version)")]
        public async Task CanCreateATokenWithReceipt()
        {
            await using var fxTreasury = await TestAccount.CreateAsync(_network);
            await using var fxRenew = await TestAccount.CreateAsync(_network);
            await using var client = _network.NewClient();
            var createParams = new CreateTokenParams
            {
                Name = Generator.Code(50),
                Symbol = Generator.UppercaseAlphaCode(20),
                Circulation = (ulong)(Generator.Integer(10, 20) * 100000),
                Decimals = (uint)Generator.Integer(2, 5),
                Treasury = fxTreasury.Record.Address,
                Administrator = fxTreasury.PublicKey,
                GrantKycEndorsement = fxTreasury.PublicKey,
                SuspendEndorsement = fxTreasury.PublicKey,
                ConfiscateEndorsement = fxTreasury.PublicKey,
                SupplyEndorsement = fxTreasury.PublicKey,
                CommissionsEndorsement = fxTreasury.PublicKey,
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
            Assert.Equal(createParams.Circulation, info.Circulation);
            Assert.Equal(createParams.Decimals, info.Decimals);
            Assert.Equal(createParams.Ceiling, info.Ceiling);
            Assert.Equal(createParams.Administrator, info.Administrator);
            Assert.Equal(createParams.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(createParams.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(createParams.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(createParams.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(createParams.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(createParams.Memo, info.Memo);
        }
        [Fact(DisplayName = "Create Token: Zero Circulation Raises Error")]
        public async Task ZeroCirculationRaisesError()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Circulation = 0;
                });
            });
            Assert.Equal("Circulation", aoe.ParamName);
            Assert.StartsWith("The initial circulation of tokens must be greater than zero.", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Zero Divisibility is Allowed")]
        public async Task ZeroDivisibilityIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Decimals = 0;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(0ul, info.Decimals);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
        }
        [Fact(DisplayName = "Create Token: Missing Treasury Address Raises Error")]
        public async Task MissingTreasuryAddressRaisesError()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Treasury = null;
                });
            });
            Assert.Equal("Treasury", aoe.ParamName);
            Assert.StartsWith("The treasury must be specified.", aoe.Message);

            aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await using var fx = await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Treasury = Address.None;
                });
            });
            Assert.Equal("Treasury", aoe.ParamName);
            Assert.StartsWith("The treasury must be specified.", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: File and Contract Address as Treasury Raises Error")]
        public async Task FileAddressAsTreasuryRaisesError()
        {
            var fxFile = await TestFile.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.Treasury = fxFile.Record.File;
                });
            });
            Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);
            Assert.StartsWith("Unable to create Token, status: InvalidTreasuryAccountForToken", tex.Message);
        }
        [Fact(DisplayName = "Create Token: Can Set Treasury to Contract Account")]
        public async Task CanSetTreasuryToNodeContractAccount()
        {
            var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Treasury = fxContract.ContractRecord.Contract;
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fxContract.PrivateKey);
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);

            var treasury = await fxToken.Client.GetContractBalancesAsync(fxContract.ContractRecord.Contract);
            var tokens = treasury.Tokens.GetValueOrDefault(fxToken.Record.Token);
            Assert.Equal(fxToken.Params.Circulation, tokens);
        }
        [Fact(DisplayName = "Create Token: Null Administrator Key is Allowed")]
        public async Task NullAdministratorKeyIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = null;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.Administrator);
        }
        [Fact(DisplayName = "Create Token: Empty Key Administrator Key is Allowed")]
        public async Task EmptyKeyAdministratorKeyIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = Endorsement.None;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.Administrator);
        }
        [Fact(DisplayName = "Create Token: Null GrantKyc Key is Allowed")]
        public async Task NullGrantKycKeyIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.GrantKycEndorsement = null;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.GrantKycEndorsement);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        }
        [Fact(DisplayName = "Create Token: Empty Key GrantKyc Key is Allowed")]
        public async Task EmptyKeyGrantKycKeyIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.GrantKycEndorsement = Endorsement.None;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.GrantKycEndorsement);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        }
        [Fact(DisplayName = "Create Token: Null Suspend Key is Allowed")]
        public async Task NullSuspendKeyIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.SuspendEndorsement = null;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Null(info.SuspendEndorsement);
            Assert.Equal(TokenTradableStatus.NotApplicable, info.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Empty Key Suspend Key is Allowed")]
        public async Task EmptyKeySuspendKeyIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.SuspendEndorsement = Endorsement.None;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Null(info.SuspendEndorsement);
            Assert.Equal(TokenTradableStatus.NotApplicable, info.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Null Confiscate Key is Allowed")]
        public async Task NullConfiscateKeyIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.ConfiscateEndorsement = null;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Null(info.ConfiscateEndorsement);
        }
        [Fact(DisplayName = "Create Token: Empty Confiscate Key is Allowed")]
        public async Task EmptyConfiscateKeyIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.ConfiscateEndorsement = Endorsement.None;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Null(info.ConfiscateEndorsement);
        }
        [Fact(DisplayName = "Create Token: Null Supply Key is Allowed")]
        public async Task NullSupplyKeyIsAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.SupplyEndorsement = null;
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.SupplyEndorsement);
        }
        [Fact(DisplayName = "Create Token: Empty Supply Key is Allowed")]
        public async Task EmptySupplyKeyIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.SupplyEndorsement = Endorsement.None;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Null(info.SupplyEndorsement);
        }
        [Fact(DisplayName = "Create Token: Null Symbol is Not Allowed")]
        public async Task NullSymbolIsNotAllowed()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await using var fx = await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Symbol = null;
                });
            });
            Assert.Equal("Symbol", aoe.ParamName);
            Assert.StartsWith("The token symbol must be specified. (Parameter 'Symbol')", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Empty Symbol is Not Allowed")]
        public async Task EmptySymbolIsNotAllowed()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await using var fx = await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Symbol = string.Empty;
                });
            });
            Assert.Equal("Symbol", aoe.ParamName);
            Assert.StartsWith("The token symbol must be specified. (Parameter 'Symbol')", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Symbol Does Numbers")]
        public async Task SymbolDoesAllowNumbers()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Symbol = "123" + Generator.UppercaseAlphaCode(20) + "456";
            });
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Duplicate Symbols Are Allowed")]
        public async Task TaskDuplicateSymbolsAreAllowed()
        {
            await using var fxTaken = await TestToken.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Symbol = fxTaken.Params.Symbol;
            });

            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Null Name is Not Allowed")]
        public async Task NullNameIsNotAllowed()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await using var fx = await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Name = null;
                });
            });
            Assert.Equal("Name", aoe.ParamName);
            Assert.StartsWith("The name cannot be null or empty. (Parameter 'Name')", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Empty Name is Not Allowed")]
        public async Task EmptyNameIsNotAllowed()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await using var fx = await TestToken.CreateAsync(_network, ctx =>
                {
                    ctx.Params.Name = string.Empty;
                });
            });
            Assert.Equal("Name", aoe.ParamName);
            Assert.StartsWith("The name cannot be null or empty. (Parameter 'Name')", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Name Does Allow Numbers and Spaces")]
        public async Task NameDoesAllowNumbersAndSpaces()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Name = Generator.UppercaseAlphaCode(20) + " 123\r\n\t?";
            });
            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Equal(fx.Params.Name, info.Name);
        }
        [Fact(DisplayName = "Create Token: Duplicate Symbols Are Allowed")]
        public async Task TaskDuplicateNamesAreAllowed()
        {
            await using var fxTaken = await TestToken.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Name = fxTaken.Params.Name;
            });

            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Initialize Supended Can Be True")]
        public async Task InitializeSupendedCanBeFalse()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.InitializeSuspended = true;
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Equal(TokenTradableStatus.Suspended, info.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Initialize Is False By Default")]
        public async Task InitializeIsFalseByDefault()
        {
            await using var fx = await TestToken.CreateAsync(_network);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
        }
        [Fact(DisplayName = "Create Token: AutoRenew Account is not Required")]
        public async Task AutoRenewAccountIsNotRequired()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.RenewAccount = null;
                fx.Params.RenewPeriod = null;
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.TreasuryAccount.PrivateKey);
            });

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Null(info.RenewAccount);
        }
        [Fact(DisplayName = "Create Token: Missing Admin Signature Raises Error")]
        public async Task MissingAdminSignatureRaisesError()
        {
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.Signatory = new Signatory(fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to create Token, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Create Token: Missing Grant Admin Signature Is Allowed")]
        public async Task MissingGrantAdminSignatureIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.NotNull(info.GrantKycEndorsement);
        }
        [Fact(DisplayName = "Create Token: Missing Suspend Admin Signature Is Allowed")]
        public async Task MissingSuspendAdminSignatureIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.NotNull(info.SuspendEndorsement);
        }
        [Fact(DisplayName = "Create Token: Missing Confiscate Admin Signature Is Allowed")]
        public async Task MissingConfiscateAdminSignatureIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.SupplyPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.NotNull(info.ConfiscateEndorsement);
        }
        [Fact(DisplayName = "Create Token: Missing Supply Admin Signature Is Allowed")]
        public async Task MissingSupplyAdminSignatureIsAllowed()
        {
            await using var fx = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.NotNull(info.SupplyEndorsement);
        }
        [Fact(DisplayName = "Create Token: Missing Renew Account Signature Raises Error")]
        public async Task CreateTokenMissingRenewAccountSignatureRaisesError()
        {
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.GrantPrivateKey, fx.SuspendPrivateKey, fx.ConfiscatePrivateKey, fx.SupplyPrivateKey, fx.TreasuryAccount.PrivateKey);
                });
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to create Token, status: InvalidSignature", tex.Message);
        }
        [Fact(DisplayName = "Create Token: Expiration time in Past Raises Error")]
        public async Task ExpirationTimeInPastRaisesError()
        {
            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
                {
                    fx.Params.Expiration = DateTime.UtcNow.AddDays(-5);
                });
            });
            Assert.Equal("Expiration", aoe.ParamName);
            Assert.StartsWith("The expiration time must be in the future.", aoe.Message);
        }
        [Fact(DisplayName = "Create Token: Only Admin, Treasury and Renew Account Keys are Requied")]
        public async Task OnlyAdminTreasuryAndRenewAccountKeysAreRequied()
        {
            await using var fx = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fx.Record.Status);
        }
        [Fact(DisplayName = "Create Token: Can Create With ReUsed Symbol From Deleted Token")]
        public async Task CanCreateWithReUsedSymbolFromDeletedToken()
        {
            await using var fxTaken = await TestToken.CreateAsync(_network);
            await fxTaken.Client.DeleteTokenAsync(fxTaken, fxTaken.AdminPrivateKey);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Symbol = fxTaken.Params.Symbol;
            });

            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Can Create With ReUsed Name From Deleted Token")]
        public async Task CanCreateWithReUsedNameFromDeletedToken()
        {
            await using var fxTaken = await TestToken.CreateAsync(_network);
            await fxTaken.Client.DeleteTokenAsync(fxTaken, fxTaken.AdminPrivateKey);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Name = fxTaken.Params.Name;
            });

            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Can Create with Contract as Treasury")]
        public async Task CanCreateWithContractAsTreasury()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.Treasury = fxContract.ContractRecord.Contract;
                fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fxContract.PrivateKey, fx.RenewAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var treasury = await fxToken.Client.GetContractBalancesAsync(fxContract);
            Assert.Equal((ulong)fxContract.ContractParams.InitialBalance, treasury.Crypto);
            Assert.Single(treasury.Tokens);
            Assert.Equal(fxToken.Params.Circulation, treasury.Tokens[fxToken]);
            Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetContractTokenBalanceAsync(fxContract, fxToken));
        }
        [Fact(DisplayName = "Create Token: Can Create without Renewal Information")]
        public async Task CanCreateWithoutRenewalInformation()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.RenewAccount = null;
                fx.Params.RenewPeriod = default;
            });
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Null(info.RenewPeriod);
            Assert.Null(info.RenewAccount);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);

            var treasury = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasury.Balance);
            Assert.Single(treasury.Tokens);

            var tokens = treasury.Tokens[0];
            Assert.Equal(fxToken.Record.Token, tokens.Token);
            Assert.Equal(fxToken.Params.Symbol, tokens.Symbol);
            Assert.Equal(fxToken.Params.Circulation, tokens.Balance);
            Assert.Equal(fxToken.Params.Decimals, tokens.Decimals);
            Assert.Equal(TokenKycStatus.Granted, tokens.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, tokens.TradableStatus);
        }
        [Fact(DisplayName = "Create Token: Can Not Schedule a Create Token")]
        public async Task CanNotScheduleACreateToken()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await TestToken.CreateAsync(_network, fx =>
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
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
        [Fact(DisplayName = "Create Token: Can Create Token with Fixed Commission")]
        public async Task CanCreateTokenWithFixedCommission()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.FixedCommissions = new FixedCommission[]
                {
                    new FixedCommission(fx.TreasuryAccount, Address.None, 1)
                };
            });
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Single(info.FixedCommissions);
            Assert.Empty(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var commission = info.FixedCommissions[0];
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, commission.Account);
            Assert.Equal(Address.None, commission.Token);
            Assert.Equal(1, commission.Amount);

        }
        [Fact(DisplayName = "Create Token: Can Create Token with Variable Commission")]
        public async Task CanCreateTokenWithVariableCommission()
        {
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.VariableCommissions = new VariableCommission[]
                {
                    new VariableCommission(fx.TreasuryAccount, 1, 2, 1, 10)
                };
            });
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
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
            Assert.Equal(fxToken.Params.CommissionsEndorsement, info.CommissionsEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.FixedCommissions);
            Assert.Single(info.VariableCommissions);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            var commission = info.VariableCommissions[0];
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, commission.Account);
            Assert.Equal(1, commission.Numerator);
            Assert.Equal(2, commission.Denominator);
            Assert.Equal(1, commission.Minimum);
            Assert.Equal(10, commission.Maximum);
        }
    }
}
