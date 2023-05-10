using Hashgraph.Test.Fixtures;
using System.Diagnostics;
using System.Threading.Tasks;
using Xunit;
using Xunit.Abstractions;

namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class PingTests
{
    private readonly NetworkCredentials _network;
    public PingTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Ping: Can Ping the Gossip Node")]
    public async Task CanPingTheGossipNode()
    {
        await using var client = _network.NewClient();

        var stopwatch = new Stopwatch();
        stopwatch.Start();
        var elapsed = await client.PingAsync();
        stopwatch.Start();

        Assert.True(stopwatch.ElapsedMilliseconds > elapsed);
    }
}