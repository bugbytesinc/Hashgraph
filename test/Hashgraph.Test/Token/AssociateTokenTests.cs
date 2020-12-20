using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class AssociateTokenTests
    {
        private readonly NetworkCredentials _network;
        public AssociateTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate token with Account")]
        public async Task CanAssociateTokenWithAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate token with Account and get Record")]
        public async Task CanAssociateTokenWithAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var record = await fxAccount.Client.AssociateTokenWithRecordAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
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

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate token with Account (No Extra Signatory)")]
        public async Task CanAssociateTokenWithAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate token with Account and get Record (No Extra Signatory)")]
        public async Task CanAssociateTokenWithAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var record = await fxAccount.Client.AssociateTokenWithRecordAsync(fxToken.Record.Token, fxAccount.Record.Address, ctx =>
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

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate Multpile Tokens with Account")]
        public async Task CanAssociateMultipleTokensWithAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            Assert.Equal(fxToken1.Record.Token, association.Token);
            Assert.Equal(fxToken1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);
            Assert.Equal(fxToken2.Record.Token, association.Token);
            Assert.Equal(fxToken2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate Multiple Tokens with Account and get Record")]
        public async Task CanAssociateMultipleTokensWithAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

            var record = await fxAccount.Client.AssociateTokensWithRecordAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
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

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            Assert.Equal(fxToken1.Record.Token, association.Token);
            Assert.Equal(fxToken1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);
            Assert.Equal(fxToken2.Record.Token, association.Token);
            Assert.Equal(fxToken2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate Multiple Token with Account (No Extra Signatory)")]
        public async Task CanAssociateMultipleTokensWithAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxToken1 = await TestToken.CreateAsync(_network);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokensAsync(tokens, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            Assert.Equal(fxToken1.Record.Token, association.Token);
            Assert.Equal(fxToken1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);
            Assert.Equal(fxToken2.Record.Token, association.Token);
            Assert.Equal(fxToken2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate Multiple Token with Account and get Record (No Extra Signatory)")]
        public async Task CanAssociateMultipleTokensWithAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
            await using var fxToken1 = await TestToken.CreateAsync(_network);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);

            var record = await fxAccount.Client.AssociateTokensWithRecordAsync(tokens, fxAccount.Record.Address, ctx =>
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

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            Assert.Equal(fxToken1.Record.Token, association.Token);
            Assert.Equal(fxToken1.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);
            Assert.Equal(fxToken2.Record.Token, association.Token);
            Assert.Equal(fxToken2.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: No Token Balance Record Exists When not Associated")]
        public async Task NoTokenBalanceRecordExistsWhenNotAssociated()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var receipt = await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Associate Tokens: Association Requires Signing by Target Account")]
        public async Task AssociationRequiresSigningByTargetAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: InvalidSignature", tex.Message);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Associate Tokens: Association Requires Token Account")]
        public async Task AssociationRequiresTokenAccount()
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
        [Fact(DisplayName = "Associate Tokens: Association Requires Account")]
        public async Task AssociationRequiresAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, null);
            });
            Assert.Equal("account", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null or empty", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, Address.None);
            });
            Assert.Equal("account", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null or empty", ane.Message);
        }
        [Fact(DisplayName = "Associate Tokens: Associating with Deleted Account Raises Error")]
        public async Task AssociatingWithDeletedAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            await fxAccount.Client.DeleteAccountAsync(fxAccount, _network.Payer, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: AccountDeleted", tex.Message);

        }
        [Fact(DisplayName = "Associate Tokens: Associating with Duplicate Token Raises Error")]
        public async Task AssociatingWithDuplicateAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var tokens = new Address[] { fxToken.Record.Token, fxToken.Record.Token };
                await fxToken.Client.AssociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenIdRepeatedInTokenList, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: TokenIdRepeatedInTokenList", pex.Message);
        }
        [Fact(DisplayName = "Associate Tokens: Associate with Associated Token Raises Error")]
        public async Task AssociateWithAssociatedTokenRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };
                await fxAccount.Client.AssociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenAlreadyAssociatedToAccount, tex.Status);
            Assert.StartsWith("Unable to associate Token with Account, status: TokenAlreadyAssociatedToAccount", tex.Message);
        }
        [Fact(DisplayName = "Associate Tokens: Can Associate token with Contract")]
        public async Task CanAssociateTokenWithContract()
        {
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            // Assert Not Associated
            var info = await fxContract.Client.GetContractInfoAsync(fxContract);
            Assert.Null(info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token));

            var receipt = await fxContract.Client.AssociateTokenAsync(fxToken.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            info = await fxContract.Client.GetContractInfoAsync(fxContract);
            var association = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.NotNull(association);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
    }
}
