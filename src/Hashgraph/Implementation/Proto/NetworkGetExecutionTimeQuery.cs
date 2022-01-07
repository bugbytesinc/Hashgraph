using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Proto;

public sealed partial class NetworkGetExecutionTimeQuery : INetworkQuery
{
    Query INetworkQuery.CreateEnvelope()
    {
        return new Query { NetworkGetExecutionTime = this };
    }

    Func<Query, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Response>> INetworkQuery.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new NetworkService.NetworkServiceClient(channel).getExecutionTimeAsync;
    }

    void INetworkQuery.SetHeader(QueryHeader header)
    {
        Header = header;
    }

    internal NetworkGetExecutionTimeQuery(IEnumerable<TxId> transactionIds) : this()
    {
        TransactionIds.AddRange(transactionIds.Select(t => new TransactionID(t)));
    }
}