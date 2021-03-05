using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token
{
    [Collection(nameof(NetworkCredentials))]
    public class MintTokenTests
    {
        private readonly NetworkCredentials _network;
        public MintTokenTests(NetworkCredentials network, ITestOutputHelper output)
        {
            _network = network;
            _network.Output = output;
        }
        [Fact(DisplayName = "Mint Tokens: Can Mint Token Coins")]
        public async Task CanMintTokens()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var receipt = await fxToken.Client.MintTokenAsync(fxToken.Record.Token, fxToken.Params.Circulation, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(fxToken.Params.Circulation * 2, receipt.Circulation);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal(fxToken.Params.Circulation * 2, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);

            var expectedTreasury = 2 * fxToken.Params.Circulation;
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Mint Tokens: Can Mint Token Coins (No Extra Signatory)")]
        public async Task CanMintTokensWithouExtraSignatory()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var receipt = await fxToken.Client.MintTokenAsync(fxToken.Record.Token, fxToken.Params.Circulation, ctx => ctx.Signatory = new Signatory(_network.Signatory, fxToken.SupplyPrivateKey));
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(fxToken.Params.Circulation * 2, receipt.Circulation);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal(fxToken.Params.Circulation * 2, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);

            var expectedTreasury = 2 * fxToken.Params.Circulation;
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Mint Tokens: Can Mint Token Coins and get Record")]
        public async Task CanMintTokensAndGetRecord()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var record = await fxToken.Client.MintTokenWithRecordAsync(fxToken.Record.Token, fxToken.Params.Circulation, fxToken.SupplyPrivateKey);
            Assert.Equal(ResponseCode.Success, record.Status);
            Assert.False(record.Hash.IsEmpty);
            Assert.NotNull(record.Concensus);
            Assert.NotNull(record.CurrentExchangeRate);
            Assert.NotNull(record.NextExchangeRate);
            Assert.NotEmpty(record.Hash.ToArray());
            Assert.Empty(record.Memo);
            Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
            Assert.Equal(_network.Payer, record.Id.Address);
            Assert.Equal(fxToken.Params.Circulation * 2, record.Circulation);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal(fxToken.Params.Circulation * 2, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);

            var expectedTreasury = 2 * fxToken.Params.Circulation;
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Mint Tokens: Can Mint Token Coins from Any Account with Supply Key")]
        public async Task CanMintTokensFromAnyAccountWithSupplyKey()
        {
            await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 100_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);
            Assert.NotNull(fxToken.Record);
            Assert.NotNull(fxToken.Record.Token);
            Assert.True(fxToken.Record.Token.AccountNum > 0);
            Assert.Equal(ResponseCode.Success, fxToken.Record.Status);

            var receipt = await fxToken.Client.MintTokenAsync(fxToken.Record.Token, fxToken.Params.Circulation, fxToken.SupplyPrivateKey, ctx =>
            {
                ctx.Payer = fxAccount.Record.Address;
                ctx.Signatory = fxAccount.PrivateKey;
            });
            Assert.Equal(ResponseCode.Success, receipt.Status);
            Assert.Equal(fxToken.Params.Circulation * 2, receipt.Circulation);

            var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
            Assert.Equal(fxToken.Record.Token, info.Token);
            Assert.Equal(fxToken.Params.Symbol, info.Symbol);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
            // Note: we doubled the circulation
            Assert.Equal(fxToken.Params.Circulation * 2, info.Circulation);
            Assert.Equal(fxToken.Params.Decimals, info.Decimals);
            Assert.Equal(fxToken.Params.Administrator, info.Administrator);
            Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
            Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
            Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
            Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
            Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
            Assert.Equal(TokenKycStatus.Revoked, info.KycStatus);
            Assert.False(info.Deleted);

            var expectedTreasury = 2 * fxToken.Params.Circulation;
            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        }
        [Fact(DisplayName = "Mint Tokens: Mint Token Record Includes Token Transfers")]
        public async Task MintTokenRecordIncludesTokenTransfers()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);

            var amountToCreate = fxToken.Params.Circulation / 3;
            var expectedCirculation = fxToken.Params.Circulation + amountToCreate;
            var expectedTreasury = expectedCirculation;
            var treasuryMintTransfer = (long)amountToCreate;

            var record = await fxToken.Client.MintTokenWithRecordAsync(fxToken.Record.Token, amountToCreate, fxToken.SupplyPrivateKey);
            Assert.Single(record.TokenTransfers);
            Assert.Equal(expectedCirculation, record.Circulation);

            var xfer = record.TokenTransfers[0];
            Assert.Equal(fxToken.Record.Token, xfer.Token);
            Assert.Equal(fxToken.TreasuryAccount.Record.Address, xfer.Address);
            Assert.Equal(treasuryMintTransfer, xfer.Amount);

            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(expectedCirculation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
        }
        [Fact(DisplayName = "Mint Tokens: Mint Token Requires a Positive Amount")]
        public async Task MintTokenRequiresAPositiveAmount()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);

            var amountToCreate = 0ul;
            var expectedCirculation = fxToken.Params.Circulation + amountToCreate;
            var expectedTreasury = expectedCirculation;

            var aoe = await Assert.ThrowsAsync<ArgumentOutOfRangeException>(async () =>
            {
                await fxToken.Client.MintTokenWithRecordAsync(fxToken.Record.Token, amountToCreate, fxToken.SupplyPrivateKey);
            });
            Assert.Equal("amount", aoe.ParamName);
            Assert.StartsWith("The token amount must be greater than zero.", aoe.Message);

            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(expectedCirculation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
        }
        [Fact(DisplayName = "Mint Tokens: Mint Token Requires Signature by Supply Key")]
        public async Task MintTokenRequiresSignatureBySupplyKey()
        {
            await using var fxToken = await TestToken.CreateAsync(_network);

            var amountToCreate = fxToken.Params.Circulation / 3;
            var expectedCirculation = fxToken.Params.Circulation;
            var expectedTreasury = expectedCirculation;

            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.MintTokenWithRecordAsync(fxToken.Record.Token, amountToCreate);
            });
            Assert.Equal(ResponseCode.InvalidSignature, tex.Status);
            Assert.StartsWith("Unable to Mint Token Coins, status: InvalidSignature", tex.Message);

            Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
            Assert.Equal(expectedCirculation, (await fxToken.Client.GetTokenInfoAsync(fxToken)).Circulation);
        }
        [Fact(DisplayName = "Mint Tokens: Can Not Schedule Mint Token Coins")]
        public async Task CanNotScheduleMintTokenCoins()
        {
            await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
            await using var fxToken = await TestToken.CreateAsync(_network);
            var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
            {
                await fxToken.Client.MintTokenAsync(
                    fxToken.Record.Token,
                    fxToken.Params.Circulation,
                    new Signatory(
                        fxToken.SupplyPrivateKey,
                        new ScheduleParams
                        {
                            PendingPayer = fxPayer
                        }));
            });
            Assert.Equal(ResponseCode.UnschedulableTransaction, tex.Status);
            Assert.StartsWith("Unable to Mint Token Coins, status: UnschedulableTransaction", tex.Message);
        }
    }
}
