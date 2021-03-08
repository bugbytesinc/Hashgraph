using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
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

            var newSymbol = Generator.UppercaseAlphaCode(20);
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
                Expiration = DateTime.UtcNow.AddDays(90),
                RenewPeriod = fxTemplate.Params.RenewPeriod,
                RenewAccount = fxTemplate.RenewAccount,
                Signatory = new Signatory(fxToken.Params.Signatory, fxTemplate.Params.Signatory),
                Memo = fxTemplate.Params.Memo
            };

            var receipt = await fxToken.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(newSymbol, info.Symbol);
            Assert.Equal(newName, info.Name);
            Assert.Equal(fxTemplate.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
            Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Equal(fxTemplate.Params.Memo, info.Memo);
            Assert.False(info.Deleted);
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

            var newSymbol = Generator.UppercaseAlphaCode(20);
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
                Expiration = DateTime.UtcNow.AddDays(90),
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
            Assert.Equal(newSymbol, info.Symbol);
            Assert.Equal(newName, info.Name);
            Assert.Equal(fxTemplate.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
            Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxTemplate.Params.Memo, info.Memo);
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
            var newSymbol = Generator.UppercaseAlphaCode(20);

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
            var newMemo = Generator.String(30, 50);

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
            var newRenwew = TimeSpan.FromDays(89);

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
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxOther.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxOther.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);
            Assert.StartsWith("Unable to update Token, status: InvalidTreasuryAccountForToken", tex.Message);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
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
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
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
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.Params.Name, info.Name);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);

            Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
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

            Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetContractTokenBalanceAsync(fxContract, fxToken));
        }
        [Fact(DisplayName = "Update Token: Can Delete an Auto Renew Account while Used by Token")]
        public async Task RemovingAnAutoRenewAccountIsNotAllowed()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);

            var receipt = await fxToken.Client.DeleteAccountAsync(fxToken.RenewAccount, _network.Payer, fxToken.RenewAccount);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxToken.Params.RenewAccount, info.RenewAccount);
            Assert.Equal(fxToken.Params.RenewPeriod, info.RenewPeriod);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxToken.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Token: Can Not Change Treasury to Unassociated Account")]
        public async Task CanChangeTreasuryToUnassociatedAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.ConfiscateEndorsement = null;
                fx.Params.SuspendEndorsement = null;
                fx.Params.GrantKycEndorsement = null;
            });
            var totalCirculation = fxToken.Params.Circulation;

            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
            Assert.Equal(totalCirculation, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));

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
            Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);

            // Confirm it did not change the Treasury Account
            var info = await fxToken.Client.GetTokenInfoAsync(fxToken);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        }
        [Fact(DisplayName = "Update Token: Can Not Schedule Update Token")]
        public async Task CanNotScheduleUpdateToken()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);
            var newSymbol = Generator.UppercaseAlphaCode(20);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
                {
                    Token = fxToken.Record.Token,
                    Symbol = newSymbol,
                    Signatory = new Signatory(
                        fxToken.AdminPrivateKey,
                        new ScheduleParams
                        {
                            PendingPayer = fxPayer
                        })
                });
            });
            Assert.Equal(ResponseCode.UnschedulableTransaction, tex.Status);
            Assert.StartsWith("Unable to update Token, status: UnschedulableTransaction", tex.Message);
        }
    }
}
