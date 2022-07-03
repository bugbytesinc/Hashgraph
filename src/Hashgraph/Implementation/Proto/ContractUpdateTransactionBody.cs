using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class ContractUpdateTransactionBody : INetworkTransaction
{
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

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to update Contract, status: {0}", result.Receipt.Status), result);
        }
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
            updateParameters.RenewAccount is null &&
            updateParameters.Memo is null &&
            updateParameters.ProxyAccount is null &&
            updateParameters.StakedNode is null &&
            updateParameters.DeclineStakeReward is null &&
            updateParameters.AutoAssociationLimit is null)
        {
            throw new ArgumentException("The Contract Updates contains no update properties, it is blank.", nameof(updateParameters));
        }
        ContractID = new ContractID(updateParameters.Contract);
        if (updateParameters.Expiration.HasValue)
        {
            ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
        }
        if (updateParameters.Administrator is not null)
        {
            AdminKey = new Key(updateParameters.Administrator);
        }
        if (updateParameters.RenewPeriod.HasValue)
        {
            AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
        }
        if (updateParameters.RenewAccount is not null)
        {
            AutoRenewAccountId = new AccountID(updateParameters.RenewAccount);
        }
        if (!string.IsNullOrWhiteSpace(updateParameters.Memo))
        {
            MemoWrapper = updateParameters.Memo;
        }
        if (updateParameters.AutoAssociationLimit is not null)
        {
            var limit = updateParameters.AutoAssociationLimit.Value;
            if (limit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.AutoAssociationLimit), "The maximum number of auto-associaitons must be nonnegative.");
            }
            MaxAutomaticTokenAssociations = updateParameters.AutoAssociationLimit.Value;
        }
        if (updateParameters.ProxyAccount is not null)
        {
            if (updateParameters.StakedNode is not null)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.ProxyAccount), "Can not set ProxyAccount and StakedNode at the same time.");
            }
            StakedAccountId = new AccountID(updateParameters.ProxyAccount);
        }
        if (updateParameters.StakedNode is not null)
        {
            StakedNodeId = updateParameters.StakedNode.Value;
        }
        if (updateParameters.DeclineStakeReward is not null)
        {
            DeclineReward = updateParameters.DeclineStakeReward.Value;
        }
    }
}