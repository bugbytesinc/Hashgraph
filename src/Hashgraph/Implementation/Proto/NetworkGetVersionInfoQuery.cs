using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class NetworkGetVersionInfoQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { NetworkGetVersionInfo = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new NetworkService.NetworkServiceClient(channel).getVersionInfoAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }
}