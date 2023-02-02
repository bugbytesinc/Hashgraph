using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class ContractCreateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { ContractCreateInstance = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { ContractCreateInstance = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new SmartContractService.SmartContractServiceClient(channel).createContractAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to create contract, status: {0}", result.Receipt.Status), result);
        }
    }

    internal ContractCreateTransactionBody(Hashgraph.CreateContractParams createParameters) : this()
    {
        if (createParameters is null)
        {
            throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
        }
        if (createParameters.AutoAssociationLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.AutoAssociationLimit), "The maximum number of auto-associaitons must be a non-negative number.");
        }
        if (createParameters.File.IsNullOrNone())
        {
            if (createParameters.ByteCode.IsEmpty)
            {
                throw new ArgumentNullException(nameof(createParameters.File), "Both the File address and ByteCode properties missing, one must be specified.");
            }
            Initcode = ByteString.CopyFrom(createParameters.ByteCode.Span);
        }
        else if (!createParameters.ByteCode.IsEmpty)
        {
            throw new ArgumentException("Both the File address and ByteCode properties are specified, only one can be set.", nameof(createParameters.File));
        }
        else
        {
            FileID = new FileID(createParameters.File);
        }
        if (createParameters.ProxyAccount.IsNullOrNone())
        {
            if (createParameters.StakedNode > 0)
            {
                StakedNodeId = createParameters.StakedNode;
            }
        }
        else if (createParameters.StakedNode > 0)
        {
            throw new ArgumentNullException(nameof(createParameters.ProxyAccount), "Both the ProxyAccount and StakedNode properties are specified, only one can be set.");
        }
        else
        {
            StakedAccountId = new AccountID(createParameters.ProxyAccount);
        }
        AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator);
        Gas = createParameters.Gas;
        InitialBalance = createParameters.InitialBalance;
        MaxAutomaticTokenAssociations = createParameters.AutoAssociationLimit;
        AutoRenewPeriod = new Duration(createParameters.RenewPeriod);
        AutoRenewAccountId = createParameters.RenewAccount.IsNullOrNone() ? null : new AccountID(createParameters.RenewAccount);
        ConstructorParameters = ByteString.CopyFrom(Abi.EncodeArguments(createParameters.Arguments).ToArray());
        DeclineReward = createParameters.DeclineStakeReward;
        Memo = createParameters.Memo ?? "";
    }
}