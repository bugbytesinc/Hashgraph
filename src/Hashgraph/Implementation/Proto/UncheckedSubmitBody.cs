using Google.Protobuf;
using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class UncheckedSubmitBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Submit Unsafe Transaction failed, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            throw new InvalidOperationException("This is not a schedulable transaction type.");
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { UncheckedSubmit = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new NetworkService.NetworkServiceClient(channel).uncheckedSubmitAsync;
        }

        internal UncheckedSubmitBody(ReadOnlyMemory<byte> transaction) : this()
        {
            TransactionBytes = ByteString.CopyFrom(transaction.Span);
        }
    }
}
