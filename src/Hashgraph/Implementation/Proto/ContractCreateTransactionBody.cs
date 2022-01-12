using Google.Protobuf;
using Grpc.Core;
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

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
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
        if (createParameters.File is null)
        {
            throw new ArgumentNullException(nameof(createParameters.File), "The File Address containing the contract is missing, it cannot be null.");
        }
        FileID = new FileID(createParameters.File);
        AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator);
        Gas = createParameters.Gas;
        InitialBalance = createParameters.InitialBalance;
        AutoRenewPeriod = new Duration(createParameters.RenewPeriod);
        ConstructorParameters = ByteString.CopyFrom(Abi.EncodeArguments(createParameters.Arguments).ToArray());
        Memo = createParameters.Memo ?? "";
    }
}