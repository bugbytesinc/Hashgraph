using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class TokenGetNftInfoQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { TokenGetNftInfo = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new TokenService.TokenServiceClient(channel).getTokenNftInfoAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal TokenGetNftInfoQuery(Asset asset) : this()
    {

        NftID = new NftID(asset);
    }
}