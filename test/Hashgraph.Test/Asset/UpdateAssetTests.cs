using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetToken
{
    [Collection(nameof(NetworkCredentials))]
    public class UpdateAssetTests
    {
        private readonly NetworkCredentials _network;
        public UpdateAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Update Asset: Can Update Asset")]
        public async Task CanUpdateAsset()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            await using var fxTemplate = await TestAsset.CreateAsync(_network);

            await fxTemplate.TreasuryAccount.Client.AssociateTokenAsync(fxAsset, fxTemplate.TreasuryAccount, fxTemplate.TreasuryAccount);

            var newSymbol = Generator.UppercaseAlphaCode(20);
            var newName = Generator.String(20, 50);
            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                // Can't do this with NFTs, they don't transfer automatically
                //Treasury = fxTemplate.Params.Treasury,
                Administrator = fxTemplate.Params.Administrator,
                GrantKycEndorsement = fxTemplate.Params.GrantKycEndorsement,
                SuspendEndorsement = fxTemplate.Params.SuspendEndorsement,
                PauseEndorsement = fxTemplate.Params.PauseEndorsement,
                ConfiscateEndorsement = fxTemplate.Params.ConfiscateEndorsement,
                SupplyEndorsement = fxTemplate.Params.SupplyEndorsement,
                RoyaltiesEndorsement = fxTemplate.Params.RoyaltiesEndorsement,
                Symbol = newSymbol,
                Name = newName,
                Expiration = DateTime.UtcNow.AddDays(90),
                RenewPeriod = fxTemplate.Params.RenewPeriod,
                RenewAccount = fxTemplate.RenewAccount,
                Signatory = new Signatory(fxAsset.Params.Signatory, fxTemplate.Params.Signatory),
                Memo = fxTemplate.Params.Memo
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(newSymbol, info.Symbol);
            Assert.Equal(newName, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)(ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0u, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
            Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxTemplate.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxTemplate.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Royalties);
            Assert.Equal(fxTemplate.Params.Memo, info.Memo);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Update Asset: Can Update Asset and get Record")]
        public async Task CanUpdateAssetAndGetRecord()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network, ctx => ctx.Metadata = null);
            await using var fxTemplate = await TestAsset.CreateAsync(_network);

            // It looks like changing the treasury requires the receiving account to be
            // associated first, since it still has to sign the update transaction anyway,
            // this seems unecessary.
            await fxTemplate.TreasuryAccount.Client.AssociateTokenAsync(fxAsset, fxTemplate.TreasuryAccount, fxTemplate.TreasuryAccount);

            var newSymbol = Generator.UppercaseAlphaCode(20);
            var newName = Generator.String(20, 50);
            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = fxTemplate.Params.Treasury,
                Administrator = fxTemplate.Params.Administrator,
                GrantKycEndorsement = fxTemplate.Params.GrantKycEndorsement,
                SuspendEndorsement = fxTemplate.Params.SuspendEndorsement,
                PauseEndorsement = fxTemplate.Params.PauseEndorsement,
                ConfiscateEndorsement = fxTemplate.Params.ConfiscateEndorsement,
                SupplyEndorsement = fxTemplate.Params.SupplyEndorsement,
                Symbol = newSymbol,
                Name = newName,
                Expiration = DateTime.UtcNow.AddDays(90),
                RenewPeriod = fxTemplate.Params.RenewPeriod,
                RenewAccount = fxTemplate.RenewAccount,
                Signatory = new Signatory(fxAsset.Params.Signatory, fxTemplate.Params.Signatory),
                Memo = fxTemplate.Params.Memo
            };

            var record = await fxAsset.Client.UpdateTokenWithRecordAsync(updateParams);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(newSymbol, info.Symbol);
            Assert.Equal(newName, info.Name);
            Assert.Equal(fxTemplate.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal(0UL, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxTemplate.Params.Administrator, info.Administrator);
            Assert.Equal(fxTemplate.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxTemplate.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxTemplate.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxTemplate.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxTemplate.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxTemplate.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Empty Update Parameters Raises Error")]
        public async Task EmptyUpdateParametersRaisesError()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Signatory = fxAsset.Params.Signatory
            };
            var ae = await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal("updateParameters", ae.ParamName);
            Assert.StartsWith("The Topic Updates contain no update properties, it is blank.", ae.Message);
        }
        [Fact(DisplayName = "Update Asset: Can Update Treasury")]
        public async Task CanUpdateTreasury()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, ctx => ctx.Metadata = null, fxAccount);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = fxAccount.Record.Address,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, fxAccount.PrivateKey)
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAccount.Record.Address, info.Treasury);
        }
        [Fact(DisplayName = "Update Asset: Can Update Admin Endorsment")]
        public async Task CanUpdateAdminEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Administrator = newPublicKey,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, newPrivateKey)
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.Administrator, info.Administrator);
        }
        [Fact(DisplayName = "Update Asset: Can Update Grant KYC Endorsement")]
        public async Task CanUpdateGrantKycEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                GrantKycEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.GrantKycEndorsement, info.GrantKycEndorsement);
        }
        [Fact(DisplayName = "Update Asset: Can Update Suspend Endorsement")]
        public async Task CanUpdateSuspendEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                SuspendEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.SuspendEndorsement, info.SuspendEndorsement);
        }
        [Fact(DisplayName = "Update Asset: Can Update Confiscate Endorsement")]
        public async Task CanUpdateConfiscateEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                ConfiscateEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.ConfiscateEndorsement, info.ConfiscateEndorsement);
        }
        [Fact(DisplayName = "Update Asset: Can Update Supply Endorsement")]
        public async Task CanUpdateSupplyEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                SupplyEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.SupplyEndorsement, info.SupplyEndorsement);
        }
        [Fact(DisplayName = "Update Asset: Can Update Royalties Endorsement")]
        public async Task CanUpdateRoyaltiesEndorsement()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                RoyaltiesEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(updateParams.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        }
        [Fact(DisplayName = "Update Asset: Can Update Symbol")]
        public async Task CanUpdateSymbol()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newSymbol = Generator.UppercaseAlphaCode(20);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Symbol = newSymbol,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newSymbol, info.Symbol);
        }
        [Fact(DisplayName = "Update Asset: Can Update Name")]
        public async Task CanUpdateName()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newName = Generator.String(30, 50);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Name = newName,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newName, info.Name);
        }
        [Fact(DisplayName = "Update Asset: Can Update Memo")]
        public async Task CanUpdateMemo()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newMemo = Generator.String(30, 50);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Memo = newMemo,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newMemo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Can Update Memo to Empty")]
        public async Task CanUpdateMemoToEmpty()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Memo = string.Empty,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Empty(info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Can Update Expiration")]
        public async Task CanUpdateExpiration()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var newExpiration = Generator.TruncateToSeconds(DateTime.UtcNow.AddDays(365));

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Expiration = newExpiration,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newExpiration, info.Expiration);
        }
        [Fact(DisplayName = "Update Asset: Can Update Renew Period")]
        public async Task CanUpdateRenewPeriod()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newRenwew = TimeSpan.FromDays(90) + TimeSpan.FromMinutes(10);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                RenewPeriod = newRenwew,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newRenwew, info.RenewPeriod);
        }
        [Fact(DisplayName = "Update Asset: Can Update Renew Account")]
        public async Task CanUpdateRenewAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newRenwew = TimeSpan.FromDays(89);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                RenewAccount = fxAccount.Record.Address,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, fxAccount.PrivateKey)
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAccount.Record.Address, info.RenewAccount);
        }
        [Fact(DisplayName = "Update Asset: Any Account With Admin Key Can Update")]
        public async Task AnyAccountWithAdminKeyCanUpdate()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newName = Generator.String(30, 50);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Name = newName,
                Signatory = fxAsset.AdminPrivateKey
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(newName, info.Name);
        }
        [Fact(DisplayName = "Update Asset: Updates Require Admin Key")]
        public async Task UpdatesRequireAdminKey()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Name = Generator.String(30, 50)
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Updating To Used Symbol Is Allowed")]
        public async Task UpdatingToUsedSymbolIsAllowed()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            await using var fxOther = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Symbol = fxOther.Params.Symbol,
                Signatory = fxAsset.AdminPrivateKey
            };
            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxOther.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Updating To Used Name Is Allowed")]
        public async Task UpdatingToUsedNameIsAllowed()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            await using var fxOther = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Name = fxOther.Params.Name,
                Signatory = fxAsset.AdminPrivateKey
            };

            await fxAsset.Client.UpdateTokenAsync(updateParams);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxOther.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Updating To Empty Treasury Address Raises Error")]
        public async Task UpdatingToEmptyTreasuryRaisesError()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = Address.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);
            Assert.StartsWith("Unable to update Token, status: InvalidTreasuryAccountForToken", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Make Asset Immutable")]
        public async Task CannotMakeAssetImmutable()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Administrator = Endorsement.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidAdminKey, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidAdminKey", pex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
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
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Remove Grant KYC Endorsement")]
        public async Task CannotRemoveGrantKYCEndorsement()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                GrantKycEndorsement = Endorsement.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidKycKey, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidKycKey", pex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Remove Suspend Endorsement")]
        public async Task CannotRemoveSuspendEndorsement()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                SuspendEndorsement = Endorsement.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidFreezeKey, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidFreezeKey", pex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Remove Confiscate Endorsement")]
        public async Task CannotRemoveConfiscateEndorsement()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                ConfiscateEndorsement = Endorsement.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidWipeKey, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidWipeKey", pex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Remove Supply Endorsement")]
        public async Task CannotRemoveSupplyEndorsement()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                SupplyEndorsement = Endorsement.None,
                Signatory = fxAsset.AdminPrivateKey
            };

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidSupplyKey, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: InvalidSupplyKey", pex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Cannot Update Imutable Asset")]
        public async Task CannotUpdateImutableAsset()
        {
            var (newPublicKey, newPrivateKey) = Generator.KeyPair();
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Params.Administrator = null);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                SupplyEndorsement = newPublicKey,
                Signatory = fxAsset.AdminPrivateKey
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.TokenIsImmutable, tex.Status);
            Assert.StartsWith("Unable to update Token, status: TokenIsImmutable", tex.Message);
        }
        [Fact(DisplayName = "Update Asset: Updating the Treasury Without Signing Raises Error")]
        public async Task UpdatingTheTreasuryWithoutSigningRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = fxAccount.Record.Address,
                Signatory = fxAsset.AdminPrivateKey
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
        }

        [Fact(DisplayName = "Update Asset: Updating the Treasury Without Signing Without Admin Key Raises Error")]
        public async Task UpdatingTheTreasuryWithoutSigningWithoutAdminKeyRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, null, fxAccount);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = fxAccount.Record.Address,
                Signatory = fxAccount.PrivateKey
            };

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(updateParams);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to update Token, status: InvalidSignature", tex.Message);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.Params.Name, info.Name);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);

            Assert.Equal((ulong)fxAsset.Metadata.Length, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));
            Assert.Equal(0UL, await fxAsset.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
        }
        [Fact(DisplayName = "Update Asset: Can Update Treasury to Contract")]
        public async Task CanUpdateTreasuryToContract()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, ctx => ctx.Metadata = null);

            // Note: Contract did not need to sign.
            await fxContract.Client.AssociateTokenAsync(fxAsset, fxContract, fxContract.PrivateKey);

            var updateParams = new UpdateTokenParams
            {
                Token = fxAsset.Record.Token,
                Treasury = fxContract.ContractRecord.Contract,
                Signatory = new Signatory(fxAsset.AdminPrivateKey, fxContract.PrivateKey)
            };

            var receipt = await fxAsset.Client.UpdateTokenAsync(updateParams);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset.Record.Token);
            Assert.Equal(fxContract.ContractRecord.Contract, info.Treasury);
        }
        [Fact(DisplayName = "Update Asset: Can Delete an Auto Renew Account while Used by Asset")]
        public async Task RemovingAnAutoRenewAccountIsNotAllowed()
        {
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var receipt = await fxAsset.Client.DeleteAccountAsync(fxAsset.RenewAccount, _network.Payer, fxAsset.RenewAccount);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.Record.Token, info.Token);
            Assert.Equal(TokenType.Asset, info.Type);
            Assert.Equal(fxAsset.Params.Symbol, info.Symbol);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
            Assert.Equal((ulong)fxAsset.Metadata.Length, info.Circulation);
            Assert.Equal(0U, info.Decimals);
            Assert.Equal(fxAsset.Params.Ceiling, info.Ceiling);
            Assert.Equal(fxAsset.Params.Administrator, info.Administrator);
            Assert.Equal(fxAsset.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxAsset.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxAsset.Params.PauseEndorsement, info.PauseEndorsement);
            Assert.Equal(fxAsset.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxAsset.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(fxAsset.Params.RenewAccount, info.RenewAccount);
            Assert.Equal(fxAsset.Params.RenewPeriod, info.RenewPeriod);
            Assert.Equal(fxAsset.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.Empty(info.Royalties);
            Assert.False(info.Deleted);
            Assert.Equal(fxAsset.Params.Memo, info.Memo);
        }
        [Fact(DisplayName = "Update Asset: Can Not Change Treasury to Unassociated Account")]
        public async Task CanChangeTreasuryToUnassociatedAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx =>
            {
                fx.Params.ConfiscateEndorsement = null;
                fx.Params.SuspendEndorsement = null;
                fx.Params.GrantKycEndorsement = null;
            });
            var totalCirculation = (ulong)fxAsset.Metadata.Length;

            Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxAsset));
            Assert.Equal(totalCirculation, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAsset.TreasuryAccount, fxAsset));

            // Returns A Failure
            var tex = await Assert.ThrowsAnyAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(new UpdateTokenParams
                {
                    Token = fxAsset.Record.Token,
                    Treasury = fxAccount.Record.Address,
                    Signatory = new Signatory(fxAsset.AdminPrivateKey, fxAccount.PrivateKey)
                });
            });
            Assert.Equal(ResponseCode.InvalidTreasuryAccountForToken, tex.Status);

            // Confirm it did not change the Treasury Account
            var info = await fxAsset.Client.GetTokenInfoAsync(fxAsset);
            Assert.Equal(fxAsset.TreasuryAccount.Record.Address, info.Treasury);
        }
        [Fact(DisplayName = "Update Asset: Can Not Schedule Update Asset")]
        public async Task CanNotScheduleUpdateAsset()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);
            var newSymbol = Generator.UppercaseAlphaCode(20);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAsset.Client.UpdateTokenAsync(new UpdateTokenParams
                {
                    Token = fxAsset.Record.Token,
                    Symbol = newSymbol,
                    Signatory = new Signatory(
                        fxAsset.AdminPrivateKey,
                        new PendingParams
                        {
                            PendingPayer = fxPayer
                        })
                });
            });
            Assert.Equal(ResponseCode.ScheduledTransactionNotInWhitelist, tex.Status);
            Assert.StartsWith("Unable to schedule transaction, status: ScheduledTransactionNotInWhitelist", tex.Message);
        }
    }
}
