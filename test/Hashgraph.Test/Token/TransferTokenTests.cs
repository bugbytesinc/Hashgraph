#pragma warning disable CS0618 // Type or member is obsolete
using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Token;

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
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

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
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token].Balance);
        Assert.Equal(fxToken.Params.Decimals, balances.Tokens[fxToken.Record.Token].Decimals);
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
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

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
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

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
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token].Balance);
        Assert.Equal(fxToken.Params.Decimals, balances.Tokens[fxToken.Record.Token].Decimals);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins and Get Record with signatories in context param")]
    public async Task CanTransferTokensAndGetRecordWithSignatoriesInContextParam()
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
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

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
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

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
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token].Balance);
        Assert.Equal(fxToken.Params.Decimals, balances.Tokens[fxToken.Record.Token].Decimals);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins")]
    public async Task CanExecuteMultiTransferTokens()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
        var xferAmount = fxToken.Params.Circulation / 3;
        var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;
        var transfers = new TransferParams
        {
            TokenTransfers = new TokenTransfer[]
            {
                    new TokenTransfer(fxToken,fxAccount1,(long)xferAmount),
                    new TokenTransfer(fxToken,fxAccount2,(long)xferAmount),
                    new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)xferAmount)
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        };
        var receipt = await fxToken.Client.TransferAsync(transfers);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
        Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token with Record")]
    public async Task CanExecuteMultiTransferTokensWithRecord()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
        var xferAmount = fxToken.Params.Circulation / 3;
        var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;
        var transfers = new TransferParams
        {
            TokenTransfers = new TokenTransfer[]
            {
                    new TokenTransfer(fxToken, fxAccount1, (long)xferAmount),
                    new TokenTransfer(fxToken, fxAccount2, (long)xferAmount),
                    new TokenTransfer(fxToken, fxToken.TreasuryAccount, -2 * (long)xferAmount)
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        };
        var record = await fxToken.Client.TransferWithRecordAsync(transfers);
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
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

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
    [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins and Crypto")]
    public async Task CanExecuteMultiTransferTokensAndCrypto()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
        var tokenAmount = fxToken.Params.Circulation / 3;
        var expectedTreasury = fxToken.Params.Circulation - 2 * tokenAmount;
        var cryptoAmount = (long)Generator.Integer(100, 200);
        var transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( _network.Payer, -2 * cryptoAmount ),
                    new CryptoTransfer(fxAccount1, cryptoAmount ),
                    new CryptoTransfer(fxAccount2, cryptoAmount )
                },
            TokenTransfers = new TokenTransfer[]
            {
                    new TokenTransfer(fxToken,fxAccount1,(long)tokenAmount),
                    new TokenTransfer(fxToken,fxAccount2,(long)tokenAmount),
                    new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)tokenAmount)
            },
            Signatory = new Signatory(_network.Signatory, fxToken.TreasuryAccount.PrivateKey)
        };
        var receipt = await fxToken.Client.TransferAsync(transfers);
        Assert.Equal(ResponseCode.Success, receipt.Status);
        Assert.NotNull(receipt.CurrentExchangeRate);
        Assert.NotNull(receipt.NextExchangeRate);
        Assert.Equal(_network.Payer, receipt.Id.Address);

        Assert.Equal(fxAccount1.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount1.Client.GetAccountBalanceAsync(fxAccount1));
        Assert.Equal(fxAccount2.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount2.Client.GetAccountBalanceAsync(fxAccount2));
        Assert.Equal(tokenAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
        Assert.Equal(tokenAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Transfer Tokens: Can Execute Multi-Transfer Token Coins and Crypto with Record")]
    public async Task CanExecuteMultiTransferTokensAndCryptoWithRecord()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
        var tokenAmount = fxToken.Params.Circulation / 3;
        var expectedTreasury = fxToken.Params.Circulation - 2 * tokenAmount;
        var cryptoAmount = (long)Generator.Integer(100, 200);
        var transfers = new TransferParams
        {
            CryptoTransfers = new[]
                {
                    new CryptoTransfer( _network.Payer, -2 * cryptoAmount ),
                    new CryptoTransfer( fxAccount1, cryptoAmount ),
                    new CryptoTransfer( fxAccount2, cryptoAmount )
                },
            TokenTransfers = new TokenTransfer[]
            {
                    new TokenTransfer(fxToken,fxAccount1,(long)tokenAmount),
                    new TokenTransfer(fxToken,fxAccount2,(long)tokenAmount),
                    new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)tokenAmount)
            },
            Signatory = new Signatory(_network.Signatory, fxToken.TreasuryAccount.PrivateKey)
        };
        var record = await fxToken.Client.TransferWithRecordAsync(transfers);
        Assert.Equal(ResponseCode.Success, record.Status);
        Assert.False(record.Hash.IsEmpty);
        Assert.NotNull(record.Concensus);
        Assert.NotNull(record.CurrentExchangeRate);
        Assert.NotNull(record.NextExchangeRate);
        Assert.NotEmpty(record.Hash.ToArray());
        Assert.Empty(record.Memo);
        Assert.InRange(record.Fee, 0UL, ulong.MaxValue);
        Assert.Equal(_network.Payer, record.Id.Address);
        Assert.Equal(5, record.Transfers.Count);
        Assert.Equal(3, record.TokenTransfers.Count);
        Assert.Empty(record.AssetTransfers);
        Assert.Empty(record.Royalties);
        Assert.Empty(record.Associations);

        Assert.Equal(cryptoAmount, record.Transfers[fxAccount1]);
        Assert.Equal(cryptoAmount, record.Transfers[fxAccount2]);

        var xferFrom = record.TokenTransfers.First(x => x.Address == fxToken.TreasuryAccount.Record.Address);
        Assert.NotNull(xferFrom);
        Assert.Equal(fxToken.Record.Token, xferFrom.Token);
        Assert.Equal(-2 * (long)tokenAmount, xferFrom.Amount);

        var xferTo1 = record.TokenTransfers.First(x => x.Address == fxAccount1.Record.Address);
        Assert.NotNull(xferTo1);
        Assert.Equal(fxToken.Record.Token, xferFrom.Token);
        Assert.Equal((long)tokenAmount, xferTo1.Amount);

        var xferTo2 = record.TokenTransfers.First(x => x.Address == fxAccount2.Record.Address);
        Assert.NotNull(xferTo2);
        Assert.Equal(fxToken.Record.Token, xferFrom.Token);
        Assert.Equal((long)tokenAmount, xferTo2.Amount);

        Assert.Equal(fxAccount1.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount1.Client.GetAccountBalanceAsync(fxAccount1));
        Assert.Equal(fxAccount2.CreateParams.InitialBalance + (ulong)cryptoAmount, await fxAccount2.Client.GetAccountBalanceAsync(fxAccount2));
        Assert.Equal(tokenAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
        Assert.Equal(tokenAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
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
        Assert.StartsWith("Unable to execute transfers, status: InvalidSignature", tex.Message);

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
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

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
        Assert.StartsWith("Unable to execute transfers, status: AccountKycNotGrantedForToken", tex.Message);

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
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

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
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

        // Can we suspend the treasury?
        await fxToken.Client.SuspendTokenAsync(fxToken, fxToken.TreasuryAccount, fxToken.SuspendPrivateKey);
        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxToken.TreasuryAccount, (long)xferAmount, fxAccount2);
        });
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

        // Double Check can't send from frozen treasury.
        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

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
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

        // Can we suspend the treasury?
        await fxToken.Client.SuspendTokenAsync(fxToken, fxToken.TreasuryAccount, fxToken.SuspendPrivateKey);
        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxAccount2, fxToken.TreasuryAccount, (long)xferAmount, fxAccount2);
        });
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

        // Double Check can't send from frozen treasury.
        tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount2, (long)xferAmount, fxToken.TreasuryAccount);
        });
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Status);
        Assert.Equal(ResponseCode.AccountFrozenForToken, tex.Receipt.Status);
        Assert.StartsWith("Unable to execute transfers, status: AccountFrozenForToken", tex.Message);

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
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens to Contract")]
    public async Task CanTransferTokensToContract()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.InitializeSuspended = false;
        });
        await fxContract.Client.AssociateTokenAsync(fxToken.Record.Token, fxContract.ContractRecord.Contract, fxContract.PrivateKey);
        var xferAmount = fxToken.Params.Circulation / 3;

        var receipt = await fxContract.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxContract.ContractRecord.Contract, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxContract.Client.GetContractInfoAsync(fxContract);
        var association = info.Tokens.FirstOrDefault(t => t.Token == fxToken.Record.Token);
        Assert.NotNull(association);
        Assert.Equal(fxToken.Record.Token, association.Token);
        Assert.Equal(fxToken.Params.Symbol, association.Symbol);
        Assert.Equal(xferAmount, association.Balance);
        Assert.Equal(fxToken.Params.Decimals, association.Decimals);
        Assert.Equal(TokenKycStatus.NotApplicable, association.KycStatus);
        Assert.Equal(TokenTradableStatus.Tradable, association.TradableStatus);
        Assert.False(association.AutoAssociated);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins to Contract and Back")]
    public async Task CanTransferTokensToContractAndBack()
    {
        await using var fxContract = await GreetingContract.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        var totalCirculation = fxToken.Params.Circulation;
        var xferAmount = 2 * fxToken.Params.Circulation / 3;
        var expectedTreasuryBalance = totalCirculation - xferAmount;

        await fxToken.Client.AssociateTokenAsync(fxToken, fxContract, fxContract.PrivateKey);

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxContract.ContractRecord.Contract, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var contractBalance = await fxContract.Client.GetContractBalancesAsync(fxContract.ContractRecord.Contract);
        Assert.Equal(fxContract.ContractRecord.Contract, contractBalance.Address);
        Assert.Equal(0UL, contractBalance.Crypto);
        Assert.Single(contractBalance.Tokens);
        Assert.Equal(xferAmount, contractBalance.Tokens[fxToken.Record.Token]);
        Assert.Equal(xferAmount, await fxContract.Client.GetContractTokenBalanceAsync(fxContract, fxToken));

        var treasuryBalance = await fxContract.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, treasuryBalance.Address);
        Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, treasuryBalance.Crypto);
        Assert.Single(treasuryBalance.Tokens);
        Assert.Equal(expectedTreasuryBalance, treasuryBalance.Tokens[fxToken.Record.Token]);
        Assert.Equal(expectedTreasuryBalance, await fxContract.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));

        receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxContract.ContractRecord.Contract, fxToken.TreasuryAccount.Record.Address, (long)xferAmount, fxContract.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        Assert.Equal(totalCirculation, await fxContract.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
        Assert.Equal(0UL, await fxContract.Client.GetContractTokenBalanceAsync(fxContract, fxToken));
    }
    [Fact(DisplayName = "Transfer Tokens: Can Move Coins by Moving the Treasury")]
    public async Task CanMoveCoinsByMovingTheTreasury()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 120_00_000_000);
        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.Params.GrantKycEndorsement = null;
            fx.Params.ConfiscateEndorsement = null;
        }, fxAccount);
        var circulation = fxToken.Params.Circulation;
        var xferAmount = circulation / 3;
        var partialTreasury = circulation - xferAmount;

        // Transfer a third of the treasury to the other account.
        await fxToken.Client.TransferTokensAsync(fxToken, fxToken.TreasuryAccount, fxAccount, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);

        // Double check balances.
        Assert.Equal(xferAmount, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
        Assert.Equal(partialTreasury, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));

        // Move the treasury to an existing account
        await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
        {
            Token = fxToken,
            Treasury = fxAccount,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxAccount.PrivateKey)
        });

        // All coins swept into new treasury account.
        Assert.Equal(circulation, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
        Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));

        // What does the info say now?
        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

        // Move the treasury back
        await fxToken.Client.UpdateTokenAsync(new UpdateTokenParams
        {
            Token = fxToken,
            Treasury = fxToken.TreasuryAccount,
            Signatory = new Signatory(fxToken.AdminPrivateKey, fxToken.TreasuryAccount.PrivateKey)
        });

        // All coins swept back to original treasury.
        Assert.Equal(0UL, await fxAccount.Client.GetAccountTokenBalanceAsync(fxAccount, fxToken));
        Assert.Equal(circulation, await fxAccount.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Transfer Tokens: Can Schedule Multi-Transfer Token Coins")]
    public async Task CanScheduleMultiTransferTokenCoins()
    {
        await using var fxPayer = await TestAccount.CreateAsync(_network, fx => fx.CreateParams.InitialBalance = 20_00_000_000);
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxAccount1, fxAccount2);
        var xferAmount = fxToken.Params.Circulation / 3;
        var expectedTreasury = fxToken.Params.Circulation - 2 * xferAmount;
        var transfers = new TransferParams
        {
            TokenTransfers = new TokenTransfer[]
            {
                    new TokenTransfer(fxToken,fxAccount1,(long)xferAmount),
                    new TokenTransfer(fxToken,fxAccount2,(long)xferAmount),
                    new TokenTransfer(fxToken,fxToken.TreasuryAccount,-2*(long)xferAmount)
            },
            Signatory = new Signatory(
                fxToken.TreasuryAccount.PrivateKey,
                new PendingParams
                {
                    PendingPayer = fxPayer
                })
        };
        var schedulingReceipt = await fxToken.Client.TransferAsync(transfers);
        Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);

        Assert.Equal(0UL, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
        Assert.Equal(0UL, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        Assert.Equal(fxToken.Params.Circulation, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));

        var counterReceipt = await fxPayer.Client.SignPendingTransactionAsync(schedulingReceipt.Pending.Id, fxPayer);
        Assert.Equal(ResponseCode.Success, counterReceipt.Status);

        var transferReceipt = await fxPayer.Client.GetReceiptAsync(schedulingReceipt.Pending.TxId);
        Assert.Equal(ResponseCode.Success, schedulingReceipt.Status);

        Assert.Equal(xferAmount, await fxAccount1.Client.GetAccountTokenBalanceAsync(fxAccount1, fxToken));
        Assert.Equal(xferAmount, await fxAccount2.Client.GetAccountTokenBalanceAsync(fxAccount2, fxToken));
        Assert.Equal(expectedTreasury, await fxToken.Client.GetAccountTokenBalanceAsync(fxToken.TreasuryAccount, fxToken));
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins to Alias Account")]
    public async Task CanTransferTokensToAliasAccount()
    {
        await using var fxAccount = await TestAliasAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null);
        await fxToken.Client.AssociateTokenAsync(fxToken.Record.Token, fxAccount, fxAccount.PrivateKey);
        var xferAmount = 2 * fxToken.Params.Circulation / 3;

        var receipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxAccount.Alias, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, receipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

        var balances = await fxAccount.Client.GetAccountBalancesAsync(fxAccount.CreateRecord.Address);
        Assert.Equal(fxAccount.CreateRecord.Address, balances.Address);
        Assert.True(balances.Crypto > 0);
        Assert.Single(balances.Tokens);
        Assert.Equal(xferAmount, balances.Tokens[fxToken.Record.Token]);

        balances = await fxAccount.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, balances.Address);
        Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
        Assert.Single(balances.Tokens);
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token]);
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token].Balance);
        Assert.Equal(fxToken.Params.Decimals, balances.Tokens[fxToken.Record.Token].Decimals);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Token Coins from Alias Account")]
    public async Task CanTransferTokensFromAliasAccount()
    {
        await using var fxFirstAccount = await TestAliasAccount.CreateAsync(_network);
        await using var fxSecondAccount = await TestAccount.CreateAsync(_network);
        await using var fxToken = await TestToken.CreateAsync(_network, fx => fx.Params.GrantKycEndorsement = null, fxSecondAccount);
        await fxToken.Client.AssociateTokenAsync(fxToken.Record.Token, fxFirstAccount, fxFirstAccount.PrivateKey);
        var xferAmount = 2 * fxToken.Params.Circulation / 3;

        var firstReceipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxToken.TreasuryAccount.Record.Address, fxFirstAccount.Alias, (long)xferAmount, fxToken.TreasuryAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, firstReceipt.Status);

        var secondReceipt = await fxToken.Client.TransferTokensAsync(fxToken.Record.Token, fxFirstAccount.Alias, fxSecondAccount, (long)xferAmount, fxFirstAccount.PrivateKey);
        Assert.Equal(ResponseCode.Success, secondReceipt.Status);

        var info = await fxToken.Client.GetTokenInfoAsync(fxToken.Record.Token);
        Assert.Equal(fxToken.Record.Token, info.Token);
        Assert.Equal(TokenType.Fungible, info.Type);
        Assert.Equal(fxToken.Params.Symbol, info.Symbol);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, info.Treasury);
        Assert.Equal(fxToken.Params.Circulation, info.Circulation);
        Assert.Equal(fxToken.Params.Decimals, info.Decimals);
        Assert.Equal(fxToken.Params.Ceiling, info.Ceiling);
        Assert.Equal(fxToken.Params.Administrator, info.Administrator);
        Assert.Equal(fxToken.Params.GrantKycEndorsement, info.GrantKycEndorsement);
        Assert.Equal(fxToken.Params.SuspendEndorsement, info.SuspendEndorsement);
        Assert.Equal(fxToken.Params.ConfiscateEndorsement, info.ConfiscateEndorsement);
        Assert.Equal(fxToken.Params.SupplyEndorsement, info.SupplyEndorsement);
        Assert.Equal(fxToken.Params.RoyaltiesEndorsement, info.RoyaltiesEndorsement);
        Assert.Equal(TokenTradableStatus.Tradable, info.TradableStatus);
        Assert.Equal(TokenTradableStatus.Tradable, info.PauseStatus);
        Assert.Equal(TokenKycStatus.NotApplicable, info.KycStatus);
        Assert.Empty(info.Royalties);
        Assert.False(info.Deleted);
        Assert.Equal(fxToken.Params.Memo, info.Memo);
        AssertHg.Equal(_network.Ledger, info.Ledger);

        var balances = await fxFirstAccount.Client.GetAccountBalancesAsync(fxFirstAccount.CreateRecord.Address);
        Assert.Equal(fxFirstAccount.CreateRecord.Address, balances.Address);
        Assert.Single(balances.Tokens);
        Assert.Equal(0UL, balances.Tokens[fxToken.Record.Token]);

        balances = await fxFirstAccount.Client.GetAccountBalancesAsync(fxSecondAccount.Record.Address);
        Assert.Equal(fxSecondAccount.Record.Address, balances.Address);
        Assert.Single(balances.Tokens);
        Assert.Equal(xferAmount, balances.Tokens[fxToken.Record.Token]);

        balances = await fxFirstAccount.Client.GetAccountBalancesAsync(fxToken.TreasuryAccount.Record.Address);
        Assert.Equal(fxToken.TreasuryAccount.Record.Address, balances.Address);
        Assert.Equal(fxToken.TreasuryAccount.CreateParams.InitialBalance, balances.Crypto);
        Assert.Single(balances.Tokens);
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token]);
        Assert.Equal(fxToken.Params.Circulation - xferAmount, balances.Tokens[fxToken.Record.Token].Balance);
        Assert.Equal(fxToken.Params.Decimals, balances.Tokens[fxToken.Record.Token].Decimals);
    }

    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens using Contract Using Ed25519 Based Accounts")]
    public async Task CanTransferTokensUsingContractUsingEd25519BasedAccounts()
    {
        await using var fxTreasuryAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Ed25519KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Ed25519KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.TreasuryAccount = fxTreasuryAccount;
            fx.Params.Treasury = fxTreasuryAccount.Record.Address;
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        }, fxAccount);
        await using var fxContract = await TransferTokenContract.CreateAsync(_network);
        await using var client = fxContract.Client.Clone(ctx => ctx.SignaturePrefixTrimLimit = int.MaxValue);
        await fxTreasuryAccount.Client.TransferAsync(_network.Payer, fxTreasuryAccount, 2_00_000_000);

        long xferAmount = (long)(fxToken.Params.Circulation / 3);

        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] {
                new TokenAllowance(
                    fxToken.Record.Token,
                    fxToken.TreasuryAccount.Record.Address,
                    fxContract.ContractRecord.Contract,
                    xferAmount)},
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var receipt = await client.CallContractAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 300000,
            FunctionName = "transferToken",
            FunctionArgs = new object[]
            {
                fxToken.Record.Token,
                fxToken.TreasuryAccount.Record.Address,
                fxAccount.Record.Address,
                xferAmount
            },
            //Signatory = fxToken.TreasuryAccount.PrivateKey
        }, ctx => {
            ctx.Payer = fxToken.TreasuryAccount;
            ctx.Signatory = fxToken.TreasuryAccount;
        }); ;

        var record = await fxAccount.Client.GetTransactionRecordAsync(receipt.Id) as CallContractRecord;
        Assert.Equal(ResponseCode.Success, record.Status);

        var result = record.CallResult.Result.As<long>();
        Assert.Equal((long)ResponseCode.Success, result);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - (ulong)xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, (ulong)xferAmount);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens using Contract Using Secp256k1 Based Accounts")]
    async Task CanTransferTokensUsingContractUsingSecp256k1BasedAccounts()
    {
        await using var fxTreasuryAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Secp256k1KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Secp256k1KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.TreasuryAccount = fxTreasuryAccount;
            fx.Params.Treasury = fxTreasuryAccount.Record.Address;
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        }, fxAccount);
        await using var fxContract = await TransferTokenContract.CreateAsync(_network);
        await using var client = fxContract.Client.Clone(ctx => ctx.SignaturePrefixTrimLimit = int.MaxValue);

        long xferAmount = (long)(fxToken.Params.Circulation / 3);

        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] {
                new TokenAllowance(
                    fxToken.Record.Token,
                    fxToken.TreasuryAccount.Record.Address,
                    fxContract.ContractRecord.Contract,
                    xferAmount)},
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var receipt = await client.CallContractAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 1000000,
            FunctionName = "transferToken",
            FunctionArgs = new object[]
            {
                fxToken.Record.Token,
                fxToken.TreasuryAccount.Record.Address,
                fxAccount.Record.Address,
                xferAmount
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        }); ;

        var record = await fxAccount.Client.GetTransactionRecordAsync(receipt.Id) as CallContractRecord;
        Assert.Equal(ResponseCode.Success, record.Status);

        var result = record.CallResult.Result.As<long>();
        Assert.Equal((long)ResponseCode.Success, result);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - (ulong)xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, (ulong)xferAmount);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens using Contract Using Secp256k1 Treasury")]
    async Task CanTransferTokensUsingContractUsingSecp256k1KeyPairTreasury()
    {
        await using var fxTreasuryAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Secp256k1KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Ed25519KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.TreasuryAccount = fxTreasuryAccount;
            fx.Params.Treasury = fxTreasuryAccount.Record.Address;
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        }, fxAccount);
        await using var fxContract = await TransferTokenContract.CreateAsync(_network);
        await using var client = fxContract.Client.Clone(ctx => ctx.SignaturePrefixTrimLimit = int.MaxValue);

        long xferAmount = (long)(fxToken.Params.Circulation / 3);

        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] { 
                new TokenAllowance(
                    fxToken.Record.Token, 
                    fxToken.TreasuryAccount.Record.Address, 
                    fxContract.ContractRecord.Contract, 
                    xferAmount)},
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var receipt = await client.CallContractAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 1000000,
            FunctionName = "transferToken",
            FunctionArgs = new object[]
            {
                fxToken.Record.Token,
                fxToken.TreasuryAccount.Record.Address,
                fxAccount.Record.Address,
                xferAmount
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        }); ;

        var record = await fxAccount.Client.GetTransactionRecordAsync(receipt.Id) as CallContractRecord;
        Assert.Equal(ResponseCode.Success, record.Status);

        var result = record.CallResult.Result.As<long>();
        Assert.Equal((long)ResponseCode.Success, result);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - (ulong)xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, (ulong)xferAmount);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens using Contract Using Secp256k1 Receiver")]
    async Task CanTransferTokensUsingContractUsingSecp256k1KeyPairReceiver()
    {
        await using var fxTreasuryAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Ed25519KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Secp256k1KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.TreasuryAccount = fxTreasuryAccount;
            fx.Params.Treasury = fxTreasuryAccount.Record.Address;
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        }, fxAccount);
        await using var fxContract = await TransferTokenContract.CreateAsync(_network);
        await using var client = fxContract.Client.Clone(ctx => ctx.SignaturePrefixTrimLimit = int.MaxValue);

        long xferAmount = (long)(fxToken.Params.Circulation / 3);

        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] {
                new TokenAllowance(
                    fxToken.Record.Token,
                    fxToken.TreasuryAccount.Record.Address,
                    fxContract.ContractRecord.Contract,
                    xferAmount)},
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var receipt = await client.CallContractAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 1000000,
            FunctionName = "transferToken",
            FunctionArgs = new object[]
            {
                fxToken.Record.Token,
                fxToken.TreasuryAccount.Record.Address,
                fxAccount.Record.Address,
                xferAmount
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        }); ;

        var record = await fxAccount.Client.GetTransactionRecordAsync(receipt.Id) as CallContractRecord;
        Assert.Equal(ResponseCode.Success, record.Status);

        var result = record.CallResult.Result.As<long>();
        Assert.Equal((long)ResponseCode.Success, result);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - (ulong)xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, (ulong)xferAmount);
    }
    [Fact(DisplayName = "Transfer Tokens: Can Transfer Tokens using Contract Using Secp256k1 List Receiver")]
    async Task CanTransferTokensUsingContractUsingSecp256k1KeyListReceiver()
    {
        await using var fxTreasuryAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Ed25519KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = pair.publicKey;
        });

        await using var fxAccount = await TestAccount.CreateAsync(_network, fx =>
        {
            var pair = Generator.Secp256k1KeyPair();
            fx.PublicKey = pair.publicKey;
            fx.PrivateKey = pair.privateKey;
            fx.CreateParams.Endorsement = new Endorsement(1, new[] { new Endorsement(pair.publicKey) });
        });

        await using var fxToken = await TestToken.CreateAsync(_network, fx =>
        {
            fx.TreasuryAccount = fxTreasuryAccount;
            fx.Params.Treasury = fxTreasuryAccount.Record.Address;
            fx.Params.GrantKycEndorsement = null;
            fx.Params.Signatory = new Signatory(fx.AdminPrivateKey, fx.RenewAccount.PrivateKey, fx.TreasuryAccount.PrivateKey);
        }, fxAccount);
        await using var fxContract = await TransferTokenContract.CreateAsync(_network);
        await using var client = fxContract.Client.Clone(ctx => ctx.SignaturePrefixTrimLimit = int.MaxValue);

        long xferAmount = (long)(fxToken.Params.Circulation / 3);

        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, 0);

        await fxToken.Client.AllocateAsync(new AllowanceParams
        {
            TokenAllowances = new[] {
                new TokenAllowance(
                    fxToken.Record.Token,
                    fxToken.TreasuryAccount.Record.Address,
                    fxContract.ContractRecord.Contract,
                    xferAmount)},
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var receipt = await client.CallContractAsync(new CallContractParams
        {
            Contract = fxContract.ContractRecord.Contract,
            Gas = 1000000,
            FunctionName = "transferToken",
            FunctionArgs = new object[]
            {
                fxToken.Record.Token,
                fxToken.TreasuryAccount.Record.Address,
                fxAccount.Record.Address,
                xferAmount
            },
            Signatory = fxToken.TreasuryAccount.PrivateKey
        });

        var record = await fxAccount.Client.GetTransactionRecordAsync(receipt.Id) as CallContractRecord;
        Assert.Equal(ResponseCode.Success, record.Status);

        var result = record.CallResult.Result.As<long>();
        Assert.Equal((long)ResponseCode.Success, result);

        await AssertHg.TokenBalanceAsync(fxToken, fxToken.TreasuryAccount, fxToken.Params.Circulation - (ulong)xferAmount);
        await AssertHg.TokenBalanceAsync(fxToken, fxAccount, (ulong)xferAmount);
    }
}