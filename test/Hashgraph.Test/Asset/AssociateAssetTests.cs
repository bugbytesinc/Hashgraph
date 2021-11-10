using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.AssetTokens
{
    [Collection(nameof(NetworkCredentials))]
    public class AssociateAssetTests
    {
        private readonly NetworkCredentials _network;
        public AssociateAssetTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Associate Assets: Can Associate asset with Account")]
        public async Task CanAssociateAssetWithAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0UL, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate asset with Account and get Record")]
        public async Task CanAssociateAssetWithAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network, fx => fx.Metadata = null);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var record = await fxAccount.Client.AssociateTokenWithRecordAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0u, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate asset with Account (No Extra Signatory)")]
        public async Task CanAssociateAssetWithAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate asset with Account and get Record (No Extra Signatory)")]
        public async Task CanAssociateAssetWithAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var record = await fxAccount.Client.AssociateTokenWithRecordAsync(fxAsset.Record.Token, fxAccount.Record.Address, ctx =>
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

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate Multpile Assets with Account")]
        public async Task CanAssociateMultipleAssetsWithAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset1 = await TestAsset.CreateAsync(_network);
            await using var fxAsset2 = await TestAsset.CreateAsync(_network);
            await using var fxToken3 = await TestToken.CreateAsync(_network);

            var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token, fxToken3.Record.Token };

            await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
            await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken3, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
            Assert.Equal(fxAsset1.Record.Token, association.Token);
            Assert.Equal(fxAsset1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);

            association = await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);
            Assert.Equal(fxAsset2.Record.Token, association.Token);
            Assert.Equal(fxAsset2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken3, fxAccount);
            Assert.Equal(fxToken3.Record.Token, association.Token);
            Assert.Equal(fxToken3.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(fxToken3.Params.Decimals, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate Multiple Assets with Account and get Record")]
        public async Task CanAssociateMultipleAssetsWithAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset1 = await TestAsset.CreateAsync(_network);
            await using var fxAsset2 = await TestAsset.CreateAsync(_network);

            var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

            await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
            await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

            var record = await fxAccount.Client.AssociateTokensWithRecordAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
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

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
            Assert.Equal(fxAsset1.Record.Token, association.Token);
            Assert.Equal(fxAsset1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);

            association = await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);
            Assert.Equal(fxAsset2.Record.Token, association.Token);
            Assert.Equal(fxAsset2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate Multiple Asset with Account (No Extra Signatory)")]
        public async Task CanAssociateMultipleAssetsWithAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset1 = await TestAsset.CreateAsync(_network);
            await using var fxAsset2 = await TestAsset.CreateAsync(_network);

            var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

            await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
            await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokensAsync(assets, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
            Assert.Equal(fxAsset1.Record.Token, association.Token);
            Assert.Equal(fxAsset1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);

            association = await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);
            Assert.Equal(fxAsset2.Record.Token, association.Token);
            Assert.Equal(fxAsset2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate Multiple Asset with Account and get Record (No Extra Signatory)")]
        public async Task CanAssociateMultipleAssetsWithAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxAsset1 = await TestAsset.CreateAsync(_network);
            await using var fxAsset2 = await TestAsset.CreateAsync(_network);

            var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };

            await AssertHg.AssetNotAssociatedAsync(fxAsset1, fxAccount);
            await AssertHg.AssetNotAssociatedAsync(fxAsset2, fxAccount);

            var record = await fxAccount.Client.AssociateTokensWithRecordAsync(assets, fxAccount.Record.Address, ctx =>
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

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset1, fxAccount);
            Assert.Equal(fxAsset1.Record.Token, association.Token);
            Assert.Equal(fxAsset1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);

            association = await AssertHg.AssetIsAssociatedAsync(fxAsset2, fxAccount);
            Assert.Equal(fxAsset2.Record.Token, association.Token);
            Assert.Equal(fxAsset2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: No Asset Balance Record Exists When not Associated")]
        public async Task NoAssetBalanceRecordExistsWhenNotAssociated()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.AssetIsAssociatedAsync(fxAsset, fxAccount);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Association Requires Signing by Target Account")]
        public async Task AssociationRequiresSigningByTargetAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.Equal(ResponseCode.InvalidSignature, tex.Receipt.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: InvalidSignature", tex.Message);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);
        }
        [Fact(DisplayName = "Associate Assets: Association Requires Asset Account")]
        public async Task AssociationRequiresAssetAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(Address.None, fxAccount.Record.Address);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null or empty.", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(null, fxAccount.Record.Address);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null or empty.", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokensAsync(null, fxAccount.Record.Address);
            });
            Assert.Equal("tokens", ane.ParamName);
            Assert.StartsWith("The list of tokens cannot be null.", ane.Message);

            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fxAccount.Client.AssociateTokensAsync(new Address[] { null }, fxAccount.Record.Address);
            });
            Assert.Equal("tokens", aoe.ParamName);
            Assert.StartsWith("The list of tokens cannot contain an empty or null address.", aoe.Message);
        }
        [Fact(DisplayName = "Associate Assets: Association Requires Account")]
        public async Task AssociationRequiresAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, null);
            });
            Assert.Equal("account", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null.", ane.Message);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, Address.None);
            });
            Assert.Equal(ResponseCode.InvalidAccountId, tex.Status);
            Assert.Equal(ResponseCode.InvalidAccountId, tex.Receipt.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: InvalidAccountId", tex.Message);
        }
        [Fact(DisplayName = "Associate Assets: Associating with Deleted Account Raises Error")]
        public async Task AssociatingWithDeletedAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await fxAccount.Client.DeleteAccountAsync(fxAccount, _network.Payer, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxAsset.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
            Assert.Equal(ResponseCode.AccountDeleted, tex.Receipt.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: AccountDeleted", tex.Message);
        }
        [Fact(DisplayName = "Associate Assets: Associating with Duplicate Asset Raises Error")]
        public async Task AssociatingWithDuplicateAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var assets = new Address[] { fxAsset.Record.Token, fxAsset.Record.Token };
                await fxAsset.Client.AssociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenIdRepeatedInTokenList, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: TokenIdRepeatedInTokenList", pex.Message);
        }
        [Fact(DisplayName = "Associate Assets: Associate with Associated Asset Raises Error")]
        public async Task AssociateWithAssociatedAssetRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset1 = await TestAsset.CreateAsync(_network, null, fxAccount);
            await using var fxAsset2 = await TestAsset.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                var assets = new Address[] { fxAsset1.Record.Token, fxAsset2.Record.Token };
                await fxAccount.Client.AssociateTokensAsync(assets, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenAlreadyAssociatedToAccount, tex.Status);
            Assert.Equal(ResponseCode.TokenAlreadyAssociatedToAccount, tex.Receipt.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: TokenAlreadyAssociatedToAccount", tex.Message);
        }
        [Fact(DisplayName = "Associate Assets: Can Associate asset with Contract")]
        public async Task CanAssociateAssetWithContract()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            // Assert Not Associated
            var info = await fxContract.Client.GetContractInfoAsync(fxContract);
            Assert.Null(info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token));

            var receipt = await fxContract.Client.AssociateTokenAsync(fxAsset.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            info = await fxContract.Client.GetContractInfoAsync(fxContract);
            var association = info.Tokens.FirstOrDefault(t => t.Token == fxAsset.Record.Token);
            Assert.NotNull(association);
            Assert.Equal(fxAsset.Record.Token, association.Token);
            Assert.Equal(fxAsset.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(0U, association.Decimals);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
            Assert.False(association.AutoAssociated);
        }
        [Fact(DisplayName = "Associate Assets: Can Not Schedule Associate asset with Account")]
        public async Task CanNotScheduleAssociateAssetWithAccount()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network);
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxAsset = await TestAsset.CreateAsync(_network);

            await AssertHg.AssetNotAssociatedAsync(fxAsset, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(
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
}
