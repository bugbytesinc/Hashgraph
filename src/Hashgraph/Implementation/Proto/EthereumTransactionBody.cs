using Google.Protobuf;
using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class EthereumTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        throw new InvalidOperationException("This is not a schedulable transaction type.");
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { EthereumTransaction = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
    {
        return new SmartContractService.SmartContractServiceClient(channel).callEthereumAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Contract call failed, status: {0}", result.Receipt.Status), result);
        }
    }

    internal EthereumTransactionBody(EthereumTransactionParams transactionParams) : this()
    {
        if (transactionParams is null)
        {
            throw new ArgumentNullException(nameof(transactionParams), "The call parameters are missing. Please check that the argument is not null.");
        }
        if (transactionParams.Transaction.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(transactionParams.Transaction), "The ethereum transaction must be specified.");
        }
        if (transactionParams.AdditionalGasAllowance < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(transactionParams.AdditionalGasAllowance), "The Additional Gas Allowance can not be negative.");
        }
        EthereumData = ByteString.CopyFrom(transactionParams.Transaction.Span);
        if (!transactionParams.ExtraCallData.IsNullOrNone())
        {
            CallData = new FileID(transactionParams.ExtraCallData);
        }
        MaxGasAllowance = transactionParams.AdditionalGasAllowance;
    }
}