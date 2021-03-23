using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ConsensusGetTopicInfoQuery : INetworkQuery
    {
        Query INetworkQuery.CreateEnvelope()
        {
            return new Query { ConsensusGetTopicInfo = this };
        }

        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new ConsensusService.ConsensusServiceClient(channel).getTopicInfoAsync;
        }

        void INetworkQuery.SetHeader(QueryHeader header)
        {
            Header = header;
        }

        internal ConsensusGetTopicInfoQuery(Address topic) : this()
        {
            TopicID = new TopicID(topic);
        }
    }
}
