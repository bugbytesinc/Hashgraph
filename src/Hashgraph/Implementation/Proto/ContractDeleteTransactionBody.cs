using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ContractDeleteTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to delete contract, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { ContractDeleteInstance = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ContractDeleteInstance = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new SmartContractService.SmartContractServiceClient(channel).deleteContractAsync;
        }

        internal ContractDeleteTransactionBody(Hashgraph.Address contractToDelete, Hashgraph.Address transferToAddress) : this()
        {
            if (contractToDelete is null)
            {
                throw new ArgumentNullException(nameof(contractToDelete), "Contract to Delete is missing. Please check that it is not null.");
            }
            if (transferToAddress is null)
            {
                throw new ArgumentNullException(nameof(transferToAddress), "Transfer address is missing. Please check that it is not null.");
            }
            ContractID = new ContractID(contractToDelete);
            TransferAccountID = new AccountID(transferToAddress);
        }
    }
}
