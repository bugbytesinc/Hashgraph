using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class SchedulableTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            throw new NotImplementedException();
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            throw new NotImplementedException();
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            throw new NotImplementedException();
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            throw new NotImplementedException();
        }
    }
}

