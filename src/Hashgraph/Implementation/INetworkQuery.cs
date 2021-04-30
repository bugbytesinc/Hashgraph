using Grpc.Core;
using Proto;
using System;
using System.Threading;

namespace Hashgraph.Implementation
{
    internal interface INetworkQuery
    {
        Query CreateEnvelope();
        Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> InstantiateNetworkRequestMethod(Channel channel);
        void SetHeader(QueryHeader header);        
    }
}
