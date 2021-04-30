using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class SystemDeleteTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => FileID is not null ?
            "Unable to delete file, status: {0}" :
            "Unable to delete contract, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { SystemDelete = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { SystemDelete = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return FileID is not null ?
                new FileService.FileServiceClient(channel).systemDeleteAsync :
                new SmartContractService.SmartContractServiceClient(channel).systemDeleteAsync;
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
}
