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
    public class DeleteTokenTests
    {
        private readonly NetworkCredentials _network;
        public DeleteTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Token Delete: Can Delete Token")]
        public async Task CanDeleteToken()
        {
            await using var fx = await TestToken.CreateAsync(_network);

            var record = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Anyone with Admin Key Can Delete Token")]
        public async Task AnyoneWithAdminKeyCanDeleteToken()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 30_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);

            var record = await fxToken.Client.DeleteTokenAsync(fxToken.Record.Token, fxToken.AdminPrivateKey, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Deleting Removes All Token Records")]
        public async Task DeletingRemovesAllTokenRecords()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var totalTinytokens = fxToken.Params.Circulation;
            var xferAmount = totalTinytokens / 3;

            await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);

            var record = await fxAccount.Client.DeleteTokenAsync(fxToken.Record.Token, fxToken.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.True(info.Deleted);

            var accountInfo = await fxToken.Client.GetAccountInfoAsync(fxAccount.Record.Address);
            var token = accountInfo.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Null(token);

            var treasuryInfo = await fxToken.Client.GetAccountInfoAsync(fxToken.TreasuryAccount.Record.Address);
            token = treasuryInfo.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
            Assert.Null(token);
        }
        [Fact(DisplayName = "Token Delete: Deleting Token Prevents Token Transfers")]
        public async Task DeletingTokenPreventsTokenTransfers()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var totalTinytokens = fxToken.Params.Circulation;
            var xferAmount = totalTinytokens / 3;

            await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);

            var record = await fxAccount.Client.DeleteTokenAsync(fxToken.Record.Token, fxToken.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.TokenWasDeleted, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: TokenWasDeleted", tex.Message);
        }
        [Fact(DisplayName = "Token Delete: Calling Delete Without Admin Key Raises Error")]
        public async Task CallingDeleteWithoutAdminKeyRaisesError()
        {
            await using var fx = await TestToken.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: InvalidSignature", tex.Message);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Calling Delete on an Imutable Token Raises an Error")]
        public async Task CallingDeleteOnAnImutableTokenRaisesAnError()
        {
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = null;
            });

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenIsImmutable, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: TokenIsImmutable", tex.Message);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Can Delete Token with One of Two Mult-Sig")]
        public async Task CanDeleteTokenWithOneOfTwoMultSig()
        {
            var (pubAdminKey2, privateAdminKey2) = Generator.KeyPair();
            await using var fx = await TestToken.CreateAsync(_network, ctx =>
            {
                ctx.Params.Administrator = new Endorsement(1, ctx.AdminPublicKey, pubAdminKey2);
                ctx.Params.Signatory = new Signatory(ctx.Params.Signatory, privateAdminKey2);
            });

            var record = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Deleting a Deleted Token Raises Error")]
        public async Task DeletingADeletedTokenRaiseesError()
        {
            await using var fx = await TestToken.CreateAsync(_network);

            var receipt = await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            var info = await fx.Client.GetTokenInfoAsync(fx.Record.Token);
            Assert.True(info.Deleted);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(fx.Record.Token, fx.AdminPrivateKey);
            });
            Assert.Equal(ResponseCode.TokenWasDeleted, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: TokenWasDeleted", tex.Message);
        }
        [Fact(DisplayName = "Token Delete: Calling Delete with invalid ID raises Error")]
        public async Task CallingDeleteWithInvalidIDRaisesError()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network);

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.DeleteTokenAsync(fxAccount.Record.Address, fxAccount.PrivateKey);
            });
            Assert.Equal(ResponseCode.InvalidTokenId, tex.Status);
            Assert.StartsWith("Unable to Delete Token, status: InvalidTokenId", tex.Message);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.False(info.Deleted);
        }
        [Fact(DisplayName = "Token Delete: Calling Delete with missing ID raises Error")]
        public async Task CallingDeleteWithMissingIDRaisesError()
        {
            await using var fx = await TestToken.CreateAsync(_network);
            var ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(null, fx.AdminPrivateKey);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);

            ane = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                await fx.Client.DeleteTokenAsync(Address.None, fx.AdminPrivateKey);
            });
            Assert.Equal("token", ane.ParamName);
            Assert.StartsWith("Token is missing. Please check that it is not null", ane.Message);
        }
        [Fact(DisplayName = "Token Delete: Cannot Delete Treasury while Attached to Token")]
        public async Task CannotDeleteTreasuryWhileAttachedToToken()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 30_00_000_000);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 30_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var circulation = fxToken.Params.Circulation;

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount1.Client.DeleteAccountAsync(fxToken.TreasuryAccount, fxAccount1, ctx =>
                {
                    ctx.Payer = fxAccount1;
                    ctx.Signatory = new Signatory(fxAccount1, fxToken.TreasuryAccount);
                });
            });
            Assert.Equal(ResponseCode.AccountIsTreasury, tex.Status);
            Assert.StartsWith("Unable to delete account, status: AccountIsTreasury", tex.Message);

            await fxAccount1.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, (long)fxToken.Params.Circulation, fxToken.TreasuryAccount);

            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxAccount1.Client.DeleteAccountAsync(fxToken.TreasuryAccount, fxAccount1, ctx =>
                {
                    ctx.Payer = fxAccount1;
                    ctx.Signatory = new Signatory(fxAccount1, fxToken.TreasuryAccount);
                });
            });
            Assert.Equal(ResponseCode.AccountIsTreasury, tex.Status);
            Assert.StartsWith("Unable to delete account, status: AccountIsTreasury", tex.Message);

            // Confirm Tokens still exist in account 2
            Assert.Equal(0ul, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(fxToken.Params.Circulation, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            // What does the info say,
            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
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
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);

            // Move the Treasury, hmm...don't need treasury key?
            await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
            {
                Token = fxToken,
                Treasury = fxAccount1,
                Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount1.PrivateKey)
            });

            // Double check balances
            Assert.Equal(0ul, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(fxToken.Params.Circulation, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            // What does the info say now?
            info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxAccount1.Record.Address, info.Treasury);
            Assert.Equal(fxToken.Params.Circulation, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
            Assert.False(info.Deleted);
        }
    }
}
