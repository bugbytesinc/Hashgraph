using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class TransferTokenTests
    {
        private readonly NetworkCredentials _network;
        public TransferTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins")]
        public async Task CanTransferTokens()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            var receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

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

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(xferAmount, balances.Tokens[fxToken.Record.Token]);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token]);
        }
        [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins and Get Record")]
        public async Task CanTransferTokensAndGetRecord()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(2, record.TokenTransfers.Count);

            var xferFrom = record.TokenTransfers.First(x => x.Address == fxToken.TreasuryAccount.Record.Address);
            Assert.NotNull(xferFrom);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal(-(long)xferAmount, xferFrom.Amount);

            var xferTo = record.TokenTransfers.First(x => x.Address == fxAccount.Record.Address);
            Assert.NotNull(xferTo);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo.Amount);

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

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(xferAmount, balances.Tokens[fxToken.Record.Token]);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token]);
        }
        [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins and Get Record (Without Extra Signatory)")]
        public async Task CanTransferTokensAndGetRecordNoExtraSignatory()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            var record = await fxToken.Client.TransferTokensWithRecordAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Record.Address, (long)xferAmount, ctx => ctx.Signatory = new Signatory(_network.PrivateKey, fxToken.TreasuryAccount.PrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(2, record.TokenTransfers.Count);

            var xferFrom = record.TokenTransfers.First(x => x.Address == fxToken.TreasuryAccount.Record.Address);
            Assert.NotNull(xferFrom);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal(-(long)xferAmount, xferFrom.Amount);

            var xferTo = record.TokenTransfers.First(x => x.Address == fxAccount.Record.Address);
            Assert.NotNull(xferTo);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo.Amount);

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

            var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.Record.Address);
            Assert.Equal(fxAccount.Record.Address, balances.Address);
            Assert.Equal(fxAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(xferAmount, balances.Tokens[fxToken.Record.Token]);

            balances = await fxAccount.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, balances.Address);
            Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
            Assert.Single(balances.Tokens);
            Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token]);
        }
        [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins")]
        public async Task CanExecuteMultiTransferTokens()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = fxToken.Params.Circulation / 3;
            var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;

            var transfers = new TokenTransfer[]
            {
                new TokenTransfer(fxToken,fxAccount1,(long)xferAmount),
                new TokenTransfer(fxToken,fxAccount2,(long)xferAmount),
                new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)xferAmount)
            };
            var receipt = await fxToken.Client.TransferTokensAsync(transfers, fxToken.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins with Record")]
        public async Task CanExecuteMultiTransferTokensWithRecord()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = fxToken.Params.Circulation / 3;
            var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;

            var transfers = new TokenTransfer[]
            {
                new TokenTransfer(fxToken,fxAccount1,(long)xferAmount),
                new TokenTransfer(fxToken,fxAccount2,(long)xferAmount),
                new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)xferAmount)
            };
            var record = await fxToken.Client.TransferTokensWithRecordAsync(transfers, fxToken.TreasuryAccount.PrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(3, record.TokenTransfers.Count);

            var xferFrom = record.TokenTransfers.First(x => x.Address == fxToken.TreasuryAccount.Record.Address);
            Assert.NotNull(xferFrom);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal(-2 * (long)xferAmount, xferFrom.Amount);

            var xferTo1 = record.TokenTransfers.First(x => x.Address == fxAccount1.Record.Address);
            Assert.NotNull(xferTo1);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo1.Amount);

            var xferTo2 = record.TokenTransfers.First(x => x.Address == fxAccount2.Record.Address);
            Assert.NotNull(xferTo2);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo2.Amount);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins with Record (No Extra Signatory)")]
        public async Task CanExecuteMultiTransferTokensWithRecordNoExtraSignatory()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = fxToken.Params.Circulation / 3;
            var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;

            var transfers = new TokenTransfer[]
            {
                new TokenTransfer(fxToken,fxAccount1,(long)xferAmount),
                new TokenTransfer(fxToken,fxAccount2,(long)xferAmount),
                new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)xferAmount)
            };
            var record = await fxToken.Client.TransferTokensWithRecordAsync(transfers, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.TreasuryAccount.PrivateKey));
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(3, record.TokenTransfers.Count);

            var xferFrom = record.TokenTransfers.First(x => x.Address == fxToken.TreasuryAccount.Record.Address);
            Assert.NotNull(xferFrom);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal(-2 * (long)xferAmount, xferFrom.Amount);

            var xferTo1 = record.TokenTransfers.First(x => x.Address == fxAccount1.Record.Address);
            Assert.NotNull(xferTo1);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo1.Amount);

            var xferTo2 = record.TokenTransfers.First(x => x.Address == fxAccount2.Record.Address);
            Assert.NotNull(xferTo2);
            Assert.Equal(fxToken.Record.Token, xferFrom.Token);
            Assert.Equal((long)xferAmount, xferTo2.Amount);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Can Pass A Token")]
        public async Task CanPassAToken()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = fxToken.Params.Circulation / 3;

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Receive Signature Requirement Applies to Tokens")]
        public async Task ReceiveSignatureRequirementAppliesToTokens()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network, fx =>
            {
                fx.CreateParams.RequireReceiveSignature = true;
                fx.CreateParams.Signatory = fx.PrivateKey;
            });
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = fxToken.Params.Circulation / 3;

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: InvalidSignature", tex.Message);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, new Signatory(fxAccount1, fxAccount2));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Cannot Pass to A Frozen Account")]
        public async Task CannotPassToAFrozenAccount()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            await fxToken.Client.SuspendTokenAsync(fxToken, fxAccount2, fxToken.SuspendPrivateKey);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Cannot Pass to A KYC Non Granted Account")]
        public async Task CannotPassToAKCYNonGrantedAccount()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, null, fxAccount1, fxAccount2);
            var xferAmount = 2 * fxToken.Params.Circulation / 3;

            await fxToken.Client.GrantTokenKycAsync(fxToken, fxAccount1, fxToken.GrantPrivateKey);

            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);
            });
            Assert.Equal(ResponseCode.AccountKycNotGrantedForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountKycNotGrantedForToken", tex.Message);

            Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens After Resume")]
        public async Task CanTransferTokensAfterResume()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = true;
            }, fxAccount1, fxAccount2);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            // Account one, by default should not recievie coins.
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Resume Participating Accounts
            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount1, fxToken.SuspendPrivateKey);
            await fxToken.Client.ResumeTokenAsync(fxToken.Record.Token, fxAccount2, fxToken.SuspendPrivateKey);

            // Move coins to account 2 via 1
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);

            // Check our Balances
            Assert.Equal(circulation - xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            // Suppend Account One from Receiving Coins
            await fxToken.Client.SuspendTokenAsync(fxToken.Record.Token, fxAccount1, fxToken.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount1, (long)xferAmount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Can we suspend the treasury?
            await fxToken.Client.SuspendTokenAsync(fxToken, fxToken.TreasuryAccount, fxToken.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxToken.TreasuryAccount, (long)xferAmount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Double Check can't send from frozen treasury.
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Balances should not have changed
            Assert.Equal(circulation - xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
        [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens After Suspend")]
        public async Task CannotTransferTokensAfterSuspend()
        {
            await using var fxAccount1 = await TestAccount.CreateAsync(_network);
            await using var fxAccount2 = await TestAccount.CreateAsync(_network);
            await using var fxToken = await TestToken.CreateAsync(_network, fx =>
            {
                fx.Params.GrantKycEndorsement = null;
                fx.Params.InitializeSuspended = false;
            }, fxAccount1, fxAccount2);
            var circulation = fxToken.Params.Circulation;
            var xferAmount = circulation / 3;

            // Move coins to account 2 via 1
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount1, (long)xferAmount, fxToken.TreasuryAccount);
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxAccount2, (long)xferAmount, fxAccount1);

            // Check our Balances
            Assert.Equal(circulation - xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            // Suppend Account One from Receiving Coins
            await fxToken.Client.SuspendTokenAsync(fxToken.Record.Token, fxAccount1, fxToken.SuspendPrivateKey);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount1, (long)xferAmount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Can we suspend the treasury?
            await fxToken.Client.SuspendTokenAsync(fxToken, fxToken.TreasuryAccount, fxToken.SuspendPrivateKey);
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxToken.TreasuryAccount, (long)xferAmount, fxAccount2);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Double Check can't send from frozen treasury.
            tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, (long)xferAmount, fxToken.TreasuryAccount);
            });
            Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
            Assert.StartsWith("Unable to execute token transfers, status: AccountFrozenForToken", tex.Message);

            // Balances should not have changed
            Assert.Equal(circulation - xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(xferAmount, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));

            // Resume Participating Accounts
            await fxToken.Client.ResumeTokenAsync(fxToken, fxAccount1, fxToken.SuspendPrivateKey);
            await fxToken.Client.ResumeTokenAsync(fxToken, fxToken.TreasuryAccount, fxToken.SuspendPrivateKey);

            // Move coins to back via 1
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxAccount1, (long)xferAmount, fxAccount2);
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount1, fxToken.TreasuryAccount, (long)xferAmount, fxAccount1);

            // Check our Final Balances
            Assert.Equal(circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
            Assert.Equal(0UL, await fxToken.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        }
    }
}
