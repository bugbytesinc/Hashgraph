using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class CryptoGetInfoQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { CryptoGetInfo = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).getAccountInfoAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal CryptoGetInfoQuery(Address address) : this()
    {
        AccountID = new AccountID(address);
    }
}