namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class GetNetworkInfoTests
{
    private readonly NetworkCredentials _network;
    public GetNetworkInfoTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Get Network Info: Can Get Network Version Info")]
    public async Task CanGetNetworkVersionInfo()
    {
        await using var client = _network.NewClient();

        var info = await client.GetVersionInfoAsync();
        Assert.NotNull(info);
        AssertHg.SemanticVersionGreaterOrEqualThan(new SemanticVersion(0, 21, 2), info.ApiProtobufVersion);
        AssertHg.SemanticVersionGreaterOrEqualThan(new SemanticVersion(0, 21, 2), info.HederaServicesVersion);
    }
}