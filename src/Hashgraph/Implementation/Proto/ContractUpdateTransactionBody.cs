using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ContractUpdateTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to update Contract, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { ContractUpdateInstance = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ContractUpdateInstance = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new SmartContractService.SmartContractServiceClient(channel).updateContractAsync;
        }

        internal ContractUpdateTransactionBody(UpdateContractParams updateParameters) : this()
        {
            if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Contract Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Contract is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Contract), "Contract address is missing. Please check that it is not null.");
            }
            if (updateParameters.Expiration is null &&
                updateParameters.Administrator is null &&
                updateParameters.RenewPeriod is null &&
                updateParameters.File is null &&
                updateParameters.Memo is null)
            {
                throw new ArgumentException("The Contract Updates contains no update properties, it is blank.", nameof(updateParameters));
            }
            ContractID = new ContractID(updateParameters.Contract);
            if (updateParameters.Expiration.HasValue)
            {
                ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
            }
            if (!(updateParameters.Administrator is null))
            {
                AdminKey = new Key(updateParameters.Administrator);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.File is null))
            {
                FileID = new FileID(updateParameters.File);
            }
            if (!string.IsNullOrWhiteSpace(updateParameters.Memo))
            {
                MemoWrapper = updateParameters.Memo;
            }
        }
    }
}
