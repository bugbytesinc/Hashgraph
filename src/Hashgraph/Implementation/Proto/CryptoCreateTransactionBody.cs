using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class CryptoCreateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { CryptoCreateAccount = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { CryptoCreateAccount = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).createAccountAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to create account, status: {0}", result.Receipt.Status), result);
        }
    }

    internal CryptoCreateTransactionBody(Hashgraph.CreateAccountParams createParameters) : this()
    {
        if (createParameters is null)
        {
            throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
        }
        if (createParameters.Endorsement is null)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters), "The Endorsement for the account is missing, it is required.");
        }
        if (createParameters.AutoAssociationLimit < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(createParameters.AutoAssociationLimit), "The maximum number of auto-associaitons must be a non-negative number.");
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
        Key = new Key(createParameters.Endorsement);
        InitialBalance = createParameters.InitialBalance;
        ReceiverSigRequired = createParameters.RequireReceiveSignature;
        AutoRenewPeriod = new Duration(createParameters.AutoRenewPeriod);
        AutoRenewAccount = createParameters.AutoRenewAccount.IsNullOrNone() ? null : new AccountID(createParameters.AutoRenewAccount);
        Alias = createParameters.Alias.IsNullOrNone() ? ByteString.Empty : new Key(createParameters.Alias.Endorsement).ToByteString();
        EvmAddress = createParameters.Moniker.IsNullOrNone() ? ByteString.Empty : ByteString.CopyFrom(createParameters.Moniker.Bytes.Span);
        DeclineReward = createParameters.DeclineStakeReward;
        Memo = createParameters.Memo ?? string.Empty;
        MaxAutomaticTokenAssociations = createParameters.AutoAssociationLimit;
    }
}