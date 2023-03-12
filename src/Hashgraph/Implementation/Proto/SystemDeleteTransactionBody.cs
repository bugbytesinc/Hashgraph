using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class SystemDeleteTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { SystemDelete = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { SystemDelete = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return FileID is not null ?
            new FileService.FileServiceClient(channel).systemDeleteAsync :
            new SmartContractService.SmartContractServiceClient(channel).systemDeleteAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format(
                FileID is not null ?
                    "Unable to delete file, status: {0}" :
                    "Unable to delete contract, status: {0}",
                result.Receipt.Status), result);
        }
    }

    internal static SystemDeleteTransactionBody FromContract(Address contract)
    {
        return new SystemDeleteTransactionBody
        {
            ContractID = new ContractID(contract)
        };
    }

    internal static SystemDeleteTransactionBody FromFile(Address file)
    {
        return new SystemDeleteTransactionBody
        {
            FileID = new FileID(file)
        };
    }
}