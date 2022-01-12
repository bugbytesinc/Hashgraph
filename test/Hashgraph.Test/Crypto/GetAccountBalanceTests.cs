using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetAccountBalanceTests
{
    private readonly NetworkCredentials _network;
    public GetAccountBalanceTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Get Account Balance: Can Get Balance for Account")]
    public async Task CanGetTinybarBalanceForAccountAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Payer;
        var balance = await client.GetAccountBalanceAsync(account);
        Assert.True(balance > 0, "Account Balance should be greater than zero.");
    }
    [Fact(DisplayName = "Get Account Balance: Can Get Balance for Server Node")]
    public async Task CanGetTinybarBalanceForGatewayAsync()
    {
        await using var client = _network.NewClient();
        var account = _network.Gateway;
        var balance = await client.GetAccountBalanceAsync(account);
        Assert.True(balance > 0, "Gateway Account Balance should be greater than zero.");
    }
    [Fact(DisplayName = "Get Account Balance: Missing Gateway Information Throws Exception")]
    public async Task MissingNodeInformationThrowsException()
    {
        var account = _network.Payer;
        await using var client = new Client(ctx => { ctx.Payer = _network.Payer; });
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            var balance = await client.GetAccountBalanceAsync(account);
        });
        Assert.StartsWith("The Network Gateway Node has not been configured.", ex.Message);
    }
    [Fact(DisplayName = "Get Account Balance: Missing Payer Account Does not Throw Exception (Free Query)")]
    public async Task MissingPayerAccountThrowsException()
    {
        var account = _network.Payer;
        await using var client = new Client(ctx => { ctx.Gateway = _network.Gateway; });
        var balance = await client.GetAccountBalanceAsync(account);
        Assert.True(balance > 0);
    }
    [Fact(DisplayName = "Get Account Balance: Missing Account for Balance Check Throws Exception")]
    public async Task MissingBalanceAccountThrowsException()
    {
        await using var client = _network.NewClient();
        var ex = await Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            var balance = await client.GetAccountBalanceAsync(null);
        });
        Assert.StartsWith("Account Address/Alias is missing.", ex.Message);
    }
    [Fact(DisplayName = "Get Account Balance: Invalid Account Address Throws Exception")]
    public async Task InvalidAccountAddressThrowsException()
    {
        await using var client = _network.NewClient();
        var account = new Address(0, 0, 0);
        var ex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            var balance = await client.GetAccountBalanceAsync(account);
        });
        Assert.Equal(ResponseCode.InvalidAccountId, ex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidAccount", ex.Message);
    }
    [Fact(DisplayName = "Get Account Balance: Gateway Node is Ignored because no Fee is Charged")]
    public async Task InvalidGatewayAddressThrowsException()
    {
        await using var client = _network.NewClient();
        client.Configure(cfg =>
        {
            cfg.Gateway = new Gateway($"{_network.NetworkAddress}:{_network.NetworkPort}", 0, 0, 999);
        });
        var balance = await client.GetAccountBalanceAsync(_network.Payer);
        Assert.True(balance > 0);
    }
    [Fact(DisplayName = "Get Account Balance: Can Set FeeLimit to Zero")]
    public async Task GetAccountBalanceRequiresNoFee()
    {
        await using var client = _network.NewClient();
        client.Configure(cfg =>
        {
            cfg.FeeLimit = 0;
        });
        var balance = await client.GetAccountBalanceAsync(_network.Payer);
        Assert.True(balance > 0);
    }
    [Fact(DisplayName = "Get Account Balance: Retreiving Account Balances is Free")]
    public async Task RetrievingAccountBalanceIsFree()
    {
        await using var fxAccount = await TestAccount.CreateAsync(_network);
        await using var client = _network.NewClient();
        client.Configure(ctx =>
        {
            ctx.Payer = fxAccount.Record.Address;
            ctx.Signatory = fxAccount.PrivateKey;
        });
        var account = _network.Payer;
        var balance1 = await client.GetAccountBalanceAsync(fxAccount.Record.Address);
        var balance2 = await client.GetAccountBalanceAsync(fxAccount.Record.Address);
        Assert.Equal(balance1, balance2);
        Assert.Equal(fxAccount.CreateParams.InitialBalance, balance1);
    }
    [Fact(DisplayName = "Get Account Balance: No Receipt is created for a Balance Query")]
    public async Task RetrievingAccountBalanceDoesNotCreateReceipt()
    {
        await using var client = _network.NewClient();
        var account = _network.Payer;
        var txId = client.CreateNewTxId();
        var balance = await client.GetAccountBalanceAsync(account, ctx => ctx.Transaction = txId);
        var tex = await Assert.ThrowsAsync<TransactionException>(async () =>
        {
            var receipt = await client.GetReceiptAsync(txId);
        });
        Assert.Equal(txId, tex.TxId);
        Assert.Equal(ResponseCode.ReceiptNotFound, tex.Status);
        Assert.StartsWith("Network failed to return a transaction receipt", tex.Message);
    }
    [Fact(DisplayName = "Get Account Balance: Can not get balance of Topic")]
    public async Task CanCreateATopicAsync()
    {
        await using var fx = await TestTopic.CreateAsync(_network);
        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await fx.Client.GetAccountBalanceAsync(fx.Record.Topic);
        });
        Assert.Equal(ResponseCode.InvalidAccountId, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: InvalidAccountId", pex.Message);
    }
    [Fact(DisplayName = "Get Account Balance: Can Get Balance for Alias Account")]
    public async Task CanGetTinybarBalanceForAliasAccountAsync()
    {
        await using var fx = await TestAliasAccount.CreateAsync(_network);

        var balanceByAlias = await fx.Client.GetAccountBalanceAsync(fx.Alias);
        Assert.True(balanceByAlias > 0, "Account Balance should be greater than zero.");

        var balanceByAccount = await fx.Client.GetAccountBalanceAsync(fx.CreateRecord.Address);
        Assert.Equal(balanceByAccount, balanceByAlias);
    }
}