using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class TokenGetInfoQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { TokenGetInfo = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new TokenService.TokenServiceClient(channel).getTokenInfoAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal TokenGetInfoQuery(Address token) : this()
    {
        Token = new TokenID(token);
    }
}