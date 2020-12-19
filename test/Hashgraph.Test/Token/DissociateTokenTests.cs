using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class DissociateTokenTests
    {
        private readonly NetworkCredentials _network;
        public DissociateTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate token from Account")]
        public async Task CanDissociateTokenFromAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var receipt = await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate token from Account and get Record")]
        public async Task CanDissociateTokenFromAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var record = await fxAccount.Client.DissociateTokenWithRecordAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
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

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate token from Account (No Extra Signatory)")]
        public async Task CanDissociateTokenFromAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var receipt = await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate token from Account and get Record (No Extra Signatory)")]
        public async Task CanDissociateTokenFromAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var record = await fxAccount.Client.DissociateTokenWithRecordAsync(fxToken.Record.Token, fxAccount.Record.Address, ctx =>
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

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate Multpile Tokens with Account")]
        public async Task CanDissociateMultipleTokensWithAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network, null, fxAccount);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);

            var receipt = await fxAccount.Client.DissociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate Multiple Tokens with Account and get Record")]
        public async Task CanDissociateMultipleTokensWithAccountAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network, null, fxAccount);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);

            var record = await fxAccount.Client.DissociateTokensWithRecordAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
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

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate Multiple Token with Account (No Extra Signatory)")]
        public async Task CanDissociateMultipleTokensWithAccountNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network, null, fxAccount);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);

            var receipt = await fxAccount.Client.DissociateTokensAsync(tokens, fxAccount.Record.Address, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate Multiple Token with Account and get Record (No Extra Signatory)")]
        public async Task CanDissociateMultipleTokensWithAccountAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network, null, fxAccount);

            var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };

            await AssertHg.TokenIsAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenIsAssociatedAsync(fxToken2, fxAccount);

            var record = await fxAccount.Client.DissociateTokensWithRecordAsync(tokens, fxAccount.Record.Address, ctx =>
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

            await AssertHg.TokenNotAssociatedAsync(fxToken1, fxAccount);
            await AssertHg.TokenNotAssociatedAsync(fxToken2, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: No Token Balance Record Exists When Dissociated")]
        public async Task NoTokenBalanceRecordExistsWhenDissociated()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var receipt = await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            await AssertHg.TokenNotAssociatedAsync(fxToken, fxAccount);
        }
        [Fact(DisplayName = "Dissociate Tokens: Dissociation Requires Signing by Target Account")]
        public async Task DissociationRequiresSigningByTargetAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Dissociate Token from Account, status: InvalidSignature", tex.Message);

            association = await AssertHg.TokenIsAssociatedAsync(fxToken, fxAccount);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.Revoked, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        }
        [Fact(DisplayName = "Dissociate Tokens: Dissociation Requires Token Account")]
        public async Task DissociationRequiresTokenAccount()
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
        [Fact(DisplayName = "Dissociate Tokens: Dissociation Requires Account")]
        public async Task DissociationRequiresAccount()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, null);
            });
            Assert.Equal("account", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null or empty", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, Address.None);
            });
            Assert.Equal("account", ane.ParamName);
            Assert.StartsWith("Account Address is missing. Please check that it is not null or empty", ane.Message);
        }
        [Fact(DisplayName = "Dissociate Tokens: Dissociating with Deleted Account Raises Error")]
        public async Task DissociatingWithDeletedAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            await fxAccount.Client.DeleteAccountAsync(fxAccount, _network.Payer, fxAccount);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount.Client.DissociateTokenAsync(fxToken.Record.Token, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.AccountDeleted, tex.Status);
            Assert.StartsWith("Unable to Dissociate Token from Account, status: AccountDelete", tex.Message);

        }
        [Fact(DisplayName = "Dissociate Tokens: Dissociating with Duplicate Token Raises Error")]
        public async Task DissociatingWithDuplicateAccountRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount);

            var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
            {
                var tokens = new Address[] { fxToken.Record.Token, fxToken.Record.Token };
                await fxToken.Client.DissociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenIdRepeatedInTokenList, pex.Status);
            Assert.StartsWith("Transaction Failed Pre-Check: TokenIdRepeatedInTokenList", pex.Message);
        }
        [Fact(DisplayName = "Dissociate Tokens: Dissociate with Dissociated Token Raises Error")]
        public async Task DissociateWithDissociatedTokenRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken1 = await TestToken.CreateAsync(_network, null, fxAccount);
            await using var fxToken2 = await TestToken.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                var tokens = new Address[] { fxToken1.Record.Token, fxToken2.Record.Token };
                await fxAccount.Client.DissociateTokensAsync(tokens, fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
            Assert.StartsWith("Unable to Dissociate Token from Account, status: TokenNotAssociatedToAccount", tex.Message);
        }
        [Fact(DisplayName = "Dissociate Tokens: Can Dissociate token from Contract Consent")]
        public async Task CanDissociateTokenFromContract()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxContract = await GreetingContract.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            var receipt = await fxContract.Client.AssociateTokenAsync(fxToken.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fxAccount.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(info);

            var association = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.NotNull(association);
            Assert.Equal(fxToken.Record.Token, association.Token);
            Assert.Equal(fxToken.Params.Symbol, association.Symbol);
            Assert.Equal(0UL, association.Balance);
            Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
            Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);

            receipt = await fxContract.Client.DissociateTokenAsync(fxToken.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            info = await fxAccount.Client.GetContractInfoAsync(fxContract.ContractRecord.Contract);
            Assert.NotNull(info);
            Assert.Null(info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxContract.ContractRecord.Contract, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenNotAssociatedToAccount, tex.Status);
            Assert.StartsWith("Unable to execute transfers, status: TokenNotAssociatedToAccount", tex.Message);

            Assert.Equal(0UL, await fxToken.Client.GetContractTokenBalanceAsync(fxContract, fxToken));
        }
        [Fact(DisplayName = "Token Delete: Can Delete Account Having Token Balance")]
        public async Task CanDeleteAccountHavingTokenBalance()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 60_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = (long)fxToken.Params.Circulation;

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation);

            await fxAccount1.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, xferAmount, fxToken.TreasuryAccount);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, fxToken.Params.Circulation);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, 0);

            // Can't delete the account because it has tokens associated with it.
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount1.Client.DeleteAccountAsync(fxAccount1, fxAccount2, fxAccount1.PrivateKey);
            });
            Assert.Equal(ResponseCode.TransactionRequiresZeroTokenBalances, tex.Status);
            Assert.StartsWith("Unable to delete account, status: TransactionRequiresZeroTokenBalances", tex.Message);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount1, fxToken.Params.Circulation);
            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, 0);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, 0);

            await fxAccount1.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, xferAmount, fxAccount1);
            await fxAccount1.Client.DeleteAccountAsync(fxAccount1, fxAccount2, fxAccount1.PrivateKey);

            await AssertHg.TokenBalanceAsync(fxToken, fxAccount2, fxToken.Params.Circulation);
            await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, 0);
        }
    }
}
