using Hashgraph.Extensions;
using Hashgraph.Test.Fixtures;
using System;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Extensions;

[Collection(nameof(NetworkCredentials))]
public class GetExchangeRateTests
{
    private readonly NetworkCredentials _network;
    public GetExchangeRateTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Exchange Rate: Can Retrieve Exchange Rates")]
    public async Task CanGetExchangeRates()
    {
        var client = _network.NewClient();
        var rate = await client.GetExchangeRatesAsync();
        Assert.NotNull(rate);
        Assert.NotNull(rate.Current);
        Assert.NotNull(rate.Next);
    }
    [Fact(DisplayName = "Exchange Rate: Exchange Rate Matches Transaction used to Retrieve Rate")]
    public async Task ExchangeRateMatchesRecentTransactions()
    {
        var txId = new TxId(_network.Payer, DateTime.UtcNow);
        var client = _network.NewClient();
        var rate = await client.GetExchangeRatesAsync(ctx => ctx.Transaction = txId);
        var receipt = await client.GetReceiptAsync(txId);
        Assert.Equal(rate.Current, receipt.CurrentExchangeRate);
        Assert.Equal(rate.Next, receipt.NextExchangeRate);
    }
}