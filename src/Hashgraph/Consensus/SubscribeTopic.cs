using Com.Hedera.Mirror.Api.Proto;
using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class MirrorClient
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
            subscribeParameters = RequireInputParameter.SubscribeParameters(subscribeParameters);
            await using var context = CreateChildContext(configure);
            RequireInContext.Url(context);
            var query = new ConsensusTopicQuery()
            {
                TopicID = Protobuf.ToTopicID(subscribeParameters.Topic),
                Limit = subscribeParameters.MaxCount
            };
            if (subscribeParameters.Starting.HasValue)
            {
                query.ConsensusStartTime = Protobuf.ToTimestamp(subscribeParameters.Starting.Value);
            }
            if (subscribeParameters.Ending.HasValue)
            {
                query.ConsensusEndTime = Protobuf.ToTimestamp(subscribeParameters.Ending.Value);
            }
            var service = new ConsensusService.ConsensusServiceClient(context.GetChannel());
            using var cancelTokenSource = CancellationTokenSource.CreateLinkedTokenSource(subscribeParameters.CancellationToken);
            var options = new CallOptions(cancellationToken: cancelTokenSource.Token);
            context.InstantiateOnSendingRequestHandler()(query);
            using var response = service.subscribeTopic(query, options);
            var stream = response.ResponseStream;
            var writer = subscribeParameters.MessageWriter;
            try
            {
                await ProcessResultStream(subscribeParameters.Topic);
            }
            catch (RpcException ex)
            {
                // Cancelled is an expected closing condition, not an error
                if (ex.StatusCode != StatusCode.Cancelled)
                {
                    throw new MirrorException($"Stream Terminated: {ex.StatusCode}", ex);
                }
            }
            finally
            {
                if (subscribeParameters.CompleteChannelWhenFinished)
                {
                    writer.TryComplete();
                }
            }

            async Task ProcessResultStream(Address topic)
            {
                while (await stream.MoveNext<ConsensusTopicResponse>())
                {
                    var message = Protobuf.FromConsensusTopicResponse(topic, stream.Current);
                    if (!writer.TryWrite(message))
                    {
                        while (await writer.WaitToWriteAsync())
                        {
                            if (!writer.TryWrite(message))
                            {
                                cancelTokenSource.Cancel();
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
