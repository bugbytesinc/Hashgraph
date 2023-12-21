namespace Hashgraph.Test.Internals;

[Collection(nameof(NetworkCredentials))]
public class TimeDriftTests
{
    private readonly NetworkCredentials _network;
    public TimeDriftTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }
    [Fact(DisplayName = "Time Drift: Testing Network Forces Wait on Explicit Transaction IDs Too Forward in Time")]
    public async Task NetworkForcesDelayOnTransactionForwardInTime()
    {
        await using var client = _network.NewClient();
        var account = _network.Payer;
        var startInstant = Epoch.UniqueClockNanos();
        var info = await client.GetAccountInfoAsync(account, ctx =>
        {
            ctx.Transaction = new TransactionID
            {
                AccountID = new AccountID(_network.Payer),
                TransactionValidStart = new Timestamp(new ConsensusTimeStamp(DateTime.UtcNow.AddSeconds(16)))
            }.AsTxId();
        });
        var duration = Epoch.UniqueClockNanos() - startInstant;
        Assert.InRange(duration, 500_000L, 240_000_000_000L);
    }
    [Fact(DisplayName = "Time Drift: Testing Network Forces Wait on Auto Transaction IDs Too Forward in Time")]
    public async Task SimulateTimeDrift()
    {
        await using var client = _network.NewClient();
        var startInstant = Epoch.UniqueClockNanos();
        Epoch.AddToClockDrift(-5_000_000_000);
        var info = await client.GetAccountBalanceAsync(_network.Payer, ctx => ctx.AdjustForLocalClockDrift = true);
        var duration = Epoch.UniqueClockNanos() - startInstant;
        Assert.InRange(duration, 4, 240_000_000_000L);
    }
}