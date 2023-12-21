namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetExecutionTimeTests
{
    private readonly NetworkCredentials _network;
    public GetExecutionTimeTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Get Network Info: Can Get Network Exectuion Times")]
    public async Task CanGetNetworkExecutionTimes()
    {
        await using var fxAccount1 = await TestAccount.CreateAsync(_network);
        await using var fxAccount2 = await TestAccount.CreateAsync(_network);
        await using var fxAccount3 = await TestAccount.CreateAsync(_network);

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            var times = await fxAccount1.Client.GetExecutionTimes(new[] { fxAccount1.Record.Id, fxAccount2.Record.Id, fxAccount3.Record.Id });
            Assert.Equal(3, times.Count);
        });
        Assert.Equal(ResponseCode.NotSupported, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: NotSupported", pex.Message);
    }

    [Fact(DisplayName = "Get Network Info: Invalid Transaction Id Results in Error")]
    public async Task InvalidTransactionIdResultsInError()
    {
        await using var client = _network.NewClient();
        var transactionId = client.CreateNewTxId();

        var pex = await Assert.ThrowsAsync<PrecheckException>(async () =>
        {
            await client.GetExecutionTimes(new[] { transactionId });
        });
        Assert.Equal(ResponseCode.NotSupported, pex.Status);
        Assert.StartsWith("Transaction Failed Pre-Check: NotSupported", pex.Message);
    }
}