using Com.Hedera.Mirror.Api.Proto;
using Grpc.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using Polly;

namespace Hashgraph;

public partial class MirrorGrpcClient
{
    /// <summary>
    /// Subscribes to a Topic Stream from a mirror node, placing the
    /// topic messages returned meeting the query criteria into the
    /// provided .net Channel.
    /// </summary>
    /// <param name="subscribeParameters">
    /// The details of the query, including the id of the topic, time
    /// constraint filters and the .net channel receiving the messages
    /// as they are returned from the server.
    /// </param>
    /// <param name="configure">
    /// Optional callback method providing an opportunity to modify the
    /// execution configuration for just this method call.  It is executed
    /// prior to submitting the request to the mirror node.
    /// </param>
    /// <returns>
    /// Returns only after one of the four conditions ocurr: the output channel is 
    /// completed by calling code; the cancelation token provided in the params is 
    /// signaled; the maximum number of topic messages was returned as configured in
    /// the params; or if the mirror stream faults during streaming, in which case a 
    /// <see cref="MirrorException"/> is thrown.
    /// </returns>
    /// <exception cref="ArgumentNullException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing or a parameter is invalid.</exception>
    /// <exception cref="MirrorException">If the mirror node stream faulted during request processing or upon submission.</exception>
    public async Task SubscribeTopicAsync(SubscribeTopicParams subscribeParameters, Action<IMirrorContext>? configure = null)
    {
        ValidateInputs(subscribeParameters);
        await using var context = CreateChildContext(configure);
        if (context.Uri is null)
        {
            throw new InvalidOperationException("The Mirror Node Urul has not been configured. Please check that 'Url' is set in the Mirror context.");
        }
        var query = CreateConsensusTopicQuery(subscribeParameters);
        using var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(subscribeParameters.CancellationToken);
        context.InstantiateOnSendingRequestHandler()(query);
        
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
                await ProcessResultStreamAsync(query);
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

        async Task ProcessResultStreamAsync(ConsensusTopicQuery query)
        {
            var service = new ConsensusService.ConsensusServiceClient(_context.GetChannel());
            var options = new CallOptions(cancellationToken: cancelTokenSource.Token);
            context.InstantiateOnSendingRequestHandler()(query);
            using var response = service.subscribeTopic(query, options);

            var stream = response.ResponseStream;
            while (await stream.MoveNext())
            {
                var message = stream.Current.ToTopicMessage(subscribeParameters.Topic); // Can be improved
                subscribeParameters.SubscribeMethod?.Invoke(message);
                query.ConsensusStartTime = stream.Current.ConsensusTimestamp;
                Console.WriteLine($"Received Message from Topic: {subscribeParameters.Topic} with consensus timestamp: {stream.Current.ConsensusTimestamp}");
            }
        }
    }

    private static ConsensusTopicQuery CreateConsensusTopicQuery(SubscribeTopicParams subscribeParameters)
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

    private static void ValidateInputs(SubscribeTopicParams subscribeParameters)
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
            throw new ArgumentNullException(nameof(subscribeParameters.SubscribeMethod), "The destination method missing. Please check that it is not null.");
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