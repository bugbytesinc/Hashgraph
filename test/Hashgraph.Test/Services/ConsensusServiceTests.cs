using Hashgraph.Extensions.Services;
using ConsensusService = Hashgraph.Extensions.Services.ConsensusService;

namespace Hashgraph.Test.Services;

[Collection(nameof(NetworkCredentials))]
public class ConsensusServiceTests
{
    private readonly NetworkCredentials _network;
    private readonly ConsensusService _service;
    public ConsensusServiceTests(NetworkCredentials network, ITestOutputHelper output)
    {
        _network = network;
        _network.Output = output;
        _service = new ConsensusService(_network.NewMirrorGrpcClient());
    }
    [Fact(DisplayName = "Subscribe Topic: Can Create and Fetch Topic Test Message from Stream")]
    public async Task CanSubscribeToATestTopic()
    {

        TopicMessage topicMessage = null;
        using var ctx = new CancellationTokenSource();
        await using var mirror = _network.NewMirrorGrpcClient();
        try
        {
            var subscribeTask = _service.SubscribeTopicAsync(new SubscribeTopicInput<TopicMessageTest<LoggingMessage>>
            {
                Topic = new Address(0, 0, 7714693),
                Starting = DateTime.UtcNow.AddMonths(-2),
                SubscribeMethod = SubcribeMethod,
            },cancellationToken:ctx.Token);

            ctx.CancelAfter(60000);

            await subscribeTask;

            if (topicMessage == null)
            {
                _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RETURN TOPIC IN ALLOWED TIME");
            }
            else
            {
                
            }
        }
        catch (MirrorException mex) when (mex.Code == MirrorExceptionCode.TopicNotFound)
        {
            _network.Output?.WriteLine("INDETERMINATE TEST - MIRROR NODE DID NOT RECEIVE TOPIC CREATE IN ALLOWED TIME");
            return;
        }
    }

    private Task SubcribeMethod(TopicMessage<TopicMessageTest<LoggingMessage>> topicMessage)
    {
        var convertedObject =topicMessage.Content;
        DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds((double)topicMessage.Concensus.Seconds);
        _network.Output?.WriteLine(new StringBuilder().Append("sequenceId ")
            .Append(topicMessage.SequenceNumber)
            .Append(" Data da mensagem: ")
            .Append(dateTime.ToString("MM/dd/yyyy hh:mm:ss"))
            .Append(" Message data : ")
            .Append(convertedObject.Data.Message)
            .ToString());
        return Task.CompletedTask;
    }
    
}

public class TopicMessageTest<T>
{
    public T Data { get; set; }
    public string Type { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

public class LoggingMessage
{
    public string Message { get; set; }
    public string Level { get; set; }
    public string Exception { get; set; }
}