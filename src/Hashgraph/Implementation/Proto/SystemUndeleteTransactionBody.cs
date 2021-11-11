using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class SystemUndeleteTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { SystemUndelete = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { SystemUndelete = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return FileID is not null ?
                new FileService.FileServiceClient(channel).systemUndeleteAsync :
                new SmartContractService.SmartContractServiceClient(channel).systemUndeleteAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format(
                    FileID is not null ?
                        "Unable to restore file, status: {0}" :
                        "Unable to restore contract, status: {0}",
                    result.Receipt.Status), result);
            }
        }

        internal static SystemUndeleteTransactionBody FromContract(Address contract)
        {
            return new SystemUndeleteTransactionBody
            {
                ContractID = new ContractID(contract)
            };
        }
        internal static SystemUndeleteTransactionBody FromFile(Address file)
        {
            return new SystemUndeleteTransactionBody
            {
                FileID = new FileID(file)
            };
        }
    }
}
