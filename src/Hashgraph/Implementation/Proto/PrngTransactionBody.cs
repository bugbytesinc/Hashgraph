using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class UtilPrngTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { UtilPrng = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { UtilPrng = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new UtilService.UtilServiceClient(channel).prngAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to create file, status: {0}", result.Receipt.Status), result);
        }
    }

    internal UtilPrngTransactionBody(int maxValue) : this()
    {
        if (maxValue < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(maxValue), "If specified, the maximum random value must be greater than zero.");
        }
        Range = maxValue;
    }
}