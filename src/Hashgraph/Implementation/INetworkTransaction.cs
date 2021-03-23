using Grpc.Core;
using Proto;
using System;
using System.Threading;

namespace Hashgraph.Implementation
{
    internal interface INetworkTransaction
    {
        TransactionBody CreateTransactionBody();
        SchedulableTransactionBody CreateSchedulableTransactionBody();
        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> InstantiateNetworkRequestMethod(Channel channel);
        string TransactionExceptionMessage { get; }
    }
}
