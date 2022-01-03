using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class UncheckedSubmitBody : INetworkTransaction
    {
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

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Submit Unsafe Transaction failed, status: {0}", result.Receipt.Status), result);
            }
        }

        internal UncheckedSubmitBody(ReadOnlyMemory<byte> transaction) : this()
        {
            TransactionBytes = ByteString.CopyFrom(transaction.Span);
        }
    }
}
