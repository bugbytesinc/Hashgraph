using System.Text;
using System.Text.Json;
using Com.Hedera.Mirror.Api.Proto;
using Grpc.Core;
using Polly;

namespace Hashgraph.Extensions.Services;

public interface IConsensusService
{
    Task SubscribeTopicAsync<T>(SubscribeTopicInput<T> subscribeParameters, Action<IMirrorContext>? configure = null, CancellationToken cancellationToken = default);
}

public class ConsensusService : IConsensusService
{
    private readonly IMirrorGrpcClient _mirrorGrpcClient;

    public ConsensusService(IMirrorGrpcClient mirrorGrpcClient)
    {
        _mirrorGrpcClient = mirrorGrpcClient;
    }

    public async Task SubscribeTopicAsync<T>(
        SubscribeTopicInput<T> subscribeParameters, 
        Action<IMirrorContext>? configure = null, 
        CancellationToken cancellationToken = default)
    {
        ValidateInputs(subscribeParameters);
        using var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var query = CreateInput(subscribeParameters);
        ConsensusTimeStamp lastConsensusTimeStamp;
        var policy = Policy
            .Handle<RpcException>(ex => ex.StatusCode == StatusCode.Unavailable)
            .WaitAndRetryForeverAsync(
                sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // exponential back-off
                onRetryAsync: async (exception, context) =>
                {
                    // Log the retry attempt or perform other actions
                });
        try
        {
            await policy.ExecuteAsync(async () =>
            {
                await Subscribe(subscribeParameters, configure, cancelTokenSource, query);
            });
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
        {
            // Cancelled is an expected closing condition, not an error
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.NotFound)
        {
            throw new MirrorException($"The topic with the specified address does not exist.", MirrorExceptionCode.TopicNotFound, ex);
        }
        catch (RpcException ex) when (ex.StatusCode == StatusCode.InvalidArgument)
        {
            throw new MirrorException($"The address exists, but is not a topic.", MirrorExceptionCode.InvalidTopicAddress, ex);
        }
        catch (RpcException ex)
        {
            throw new MirrorException($"Stream Terminated with Error: {ex.StatusCode}", MirrorExceptionCode.CommunicationError, ex);
        }
        finally
        { 
            await cancelTokenSource.CancelAsync();
        }
        
    }

    private async Task Subscribe<T>(SubscribeTopicInput<T> subscribeParameters, Action<IMirrorContext>? configure,
        CancellationTokenSource cancelTokenSource, ConsensusTopicQuery query)
    {
        var service = new Com.Hedera.Mirror.Api.Proto.ConsensusService.ConsensusServiceClient(_mirrorGrpcClient.GetChannel(configure));
        var options = new CallOptions(cancellationToken: cancelTokenSource.Token);
        // context.InstantiateOnSendingRequestHandler()(query); :TODO: Is this needed?
        using var response = service.subscribeTopic(query, options);

        var stream = response.ResponseStream;
        while (await stream.MoveNext())
        {
            var message = stream.Current.ToTopicMessage<T>(subscribeParameters.Topic); // Can be improved
            var stringData = Encoding.UTF8.GetString(message.Messsage.Span);
            try
            {
                var convertedObject = JsonSerializer.Deserialize<T>(stringData);
                message.SetContent(convertedObject);
            }
            catch (Exception e)
            {
                message.StringContent = stringData;
            }
            subscribeParameters.SubscribeMethod?.Invoke(message);
            query.ConsensusStartTime = stream.Current.ConsensusTimestamp;
            Console.WriteLine($"Received Message from Topic: {subscribeParameters.Topic} with consensus timestamp: {stream.Current.ConsensusTimestamp}");
        }
    }

    private static ConsensusTopicQuery CreateInput<T>(SubscribeTopicInput<T> subscribeParameters)
    {
        var query = new ConsensusTopicQuery()
        {
            TopicID = new Proto.TopicID(subscribeParameters.Topic),
            Limit = subscribeParameters.MaxCount
        };
        if (subscribeParameters.Starting.HasValue)
        {
            query.ConsensusStartTime = new Proto.Timestamp(subscribeParameters.Starting.Value);
        }
        if (subscribeParameters.Ending.HasValue)
        {
            query.ConsensusEndTime = new Proto.Timestamp(subscribeParameters.Ending.Value);
        }

        return query;
    }

    private static void ValidateInputs<T>(SubscribeTopicInput<T> subscribeParameters)
    {
        if (subscribeParameters is null)
        {
            throw new ArgumentNullException(nameof(subscribeParameters), "Topic Subscribe Parameters argument is missing. Please check that it is not null.");
        }
        if (subscribeParameters.Topic is null)
        {
            throw new ArgumentNullException(nameof(subscribeParameters.Topic), "Topic address is missing. Please check that it is not null.");
        }
        if (subscribeParameters.SubscribeMethod is null)
        {
            throw new ArgumentNullException(nameof(subscribeParameters.SubscribeMethod), "The destination channel writer missing. Please check that it is not null.");
        }
        if (subscribeParameters.Starting.HasValue && subscribeParameters.Ending.HasValue)
        {
            if (subscribeParameters.Ending.Value < subscribeParameters.Starting.Value)
            {
                throw new ArgumentOutOfRangeException(nameof(subscribeParameters.Ending), "The ending filter date is less than the starting filter date, no records can be returned.");
            }
        }
    }
}