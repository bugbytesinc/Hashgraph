using Grpc.Core;
using System;
using System.Threading;

namespace Hashgraph.Implementation;

internal interface INetworkTransaction
{
    Proto.TransactionBody CreateTransactionBody();
    Proto.SchedulableTransactionBody CreateSchedulableTransactionBody();
    Func<Proto.Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<Proto.TransactionResponse>> InstantiateNetworkRequestMethod(Channel channel);
    void CheckReceipt(NetworkResult result);
}