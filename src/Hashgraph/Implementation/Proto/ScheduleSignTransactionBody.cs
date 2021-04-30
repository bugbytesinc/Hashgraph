using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ScheduleSignTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Failed to Sign Pending Transaction, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            throw new InvalidOperationException("Can not Schedule a Sign Pending Transaction.");
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ScheduleSign = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new ScheduleService.ScheduleServiceClient(channel).signScheduleAsync;
        }

        internal ScheduleSignTransactionBody(Hashgraph.Address pending) : this()
        {
            ScheduleID = new ScheduleID(pending);
        }
    }
}
