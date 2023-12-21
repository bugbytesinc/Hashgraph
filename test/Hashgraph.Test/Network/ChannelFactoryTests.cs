namespace Hashgraph.Test.Crypto;

[Collection(nameof(NetworkCredentials))]
public class ChannelFactoryTests
{
    private readonly NetworkCredentials _network;
    public ChannelFactoryTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
    }

    [Fact(DisplayName = "Channel Factory: Can Create Client with Custom Channel Factory")]
    public async Task CanCreateClientWithCustomChannelFactory()
    {
        Gateway calledGateway = default;

        await using var client = new Client(channelFactory, ctx =>
        {
            ctx.Payer = _network.Payer;
            ctx.Gateway = _network.Gateway;
        });
        await client.PingAsync();
        Assert.Equal(_network.Gateway, calledGateway);


        GrpcChannel channelFactory(Gateway gateway)
        {
            calledGateway = gateway;
            var options = new GrpcChannelOptions()
            {
                HttpHandler = new SocketsHttpHandler
                {
                    EnableMultipleHttp2Connections = true,
                },
                DisposeHttpClient = true,
            };
            return GrpcChannel.ForAddress(gateway.Uri, options);
        }
    }

    [Fact(DisplayName = "Channel Factory: Channel Creation Error Propigates")]
    public async Task ChannelCreationErrorPropigates()
    {
        await using var client = new Client(channelFactory, ctx =>
        {
            ctx.Payer = _network.Payer;
            ctx.Gateway = _network.Gateway;
        });
        var nie = await Assert.ThrowsAsync<NotImplementedException>(async () =>
        {
            await client.PingAsync();
        });
        Assert.Equal("Channel Creation Factory Test", nie.Message);

        GrpcChannel channelFactory(Gateway gateway)
        {
            throw new NotImplementedException("Channel Creation Factory Test");
        }
    }
}