using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class ContractCallTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { ContractCall = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { ContractCall = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new SmartContractService.SmartContractServiceClient(channel).contractCallMethodAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Contract call failed, status: {0}", result.Receipt.Status), result);
        }
    }

    internal ContractCallTransactionBody(CallContractParams callParameters) : this()
    {
        if (callParameters is null)
        {
            throw new ArgumentNullException(nameof(callParameters), "The call parameters are missing. Please check that the argument is not null.");
        }
        ContractID = new ContractID(callParameters.Contract);
        Gas = callParameters.Gas;
        Amount = callParameters.PayableAmount;
        FunctionParameters = ByteString.CopyFrom(Abi.EncodeFunctionWithArguments(callParameters.FunctionName, callParameters.FunctionArgs).Span);
    }
}