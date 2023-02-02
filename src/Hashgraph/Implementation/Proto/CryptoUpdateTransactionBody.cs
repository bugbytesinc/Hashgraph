using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class CryptoUpdateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { CryptoUpdateAccount = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { CryptoUpdateAccount = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new CryptoService.CryptoServiceClient(channel).updateAccountAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to update account, status: {0}", result.Receipt.Status), result);
        }
    }

    internal CryptoUpdateTransactionBody(Hashgraph.UpdateAccountParams updateParameters) : this()
    {
        if (updateParameters is null)
        {
            throw new ArgumentNullException(nameof(updateParameters), "Account Update Parameters argument is missing. Please check that it is not null.");
        }
        if (updateParameters.Address is null)
        {
            throw new ArgumentNullException(nameof(updateParameters.Address), "Account is missing. Please check that it is not null.");
        }
        if (Endorsement.None.Equals(updateParameters.Endorsement))
        {
            throw new ArgumentOutOfRangeException(nameof(updateParameters.Endorsement), "Endorsement can not be 'None', it must contain at least one key requirement.");
        }
        if (updateParameters.Endorsement is null &&
            updateParameters.RequireReceiveSignature is null &&
            updateParameters.Expiration is null &&
            updateParameters.AutoRenewPeriod is null &&
            updateParameters.Memo is null &&
            updateParameters.AutoAssociationLimit is null &&
            updateParameters.Alias is null &&
            updateParameters.ProxyAccount is null &&
            updateParameters.StakedNode is null &&
            updateParameters.DeclineStakeReward is null &&
            updateParameters.AutoRenewAccount is null &&
            updateParameters.UpdateMoniker is null)
        {
            throw new ArgumentException("The Account Updates contains no update properties, it is blank.", nameof(updateParameters));
        }
        AccountIDToUpdate = new AccountID(updateParameters.Address);
        if (updateParameters.Endorsement is not null)
        {
            Key = new Key(updateParameters.Endorsement);
        }
        if (updateParameters.RequireReceiveSignature.HasValue)
        {
            ReceiverSigRequiredWrapper = updateParameters.RequireReceiveSignature.Value;
        }
        if (updateParameters.AutoRenewPeriod.HasValue)
        {
            AutoRenewPeriod = new Duration(updateParameters.AutoRenewPeriod.Value);
        }
        if (updateParameters.AutoRenewAccount is not null)
        {
            AutoRenewAccount = new AccountID(updateParameters.AutoRenewAccount);
        }
        if (updateParameters.Expiration.HasValue)
        {
            ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
        }
        if (updateParameters.Memo is not null)
        {
            Memo = updateParameters.Memo;
        }
        if (updateParameters.AutoAssociationLimit is not null)
        {
            var limit = updateParameters.AutoAssociationLimit.Value;
            if (limit < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.AutoAssociationLimit), "The maximum number of auto-associaitons must be nonnegative.");
            }
            MaxAutomaticTokenAssociations = limit;
        }
        if (updateParameters.Alias is not null)
        {
            //Alias = new Key(updateParameters.Alias.Endorsement).ToByteString();
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
        if (updateParameters.UpdateMoniker is not null)
        {
            var bytes = ByteString.CopyFrom(updateParameters.UpdateMoniker.Moniker.Bytes.Span);
            switch (updateParameters.UpdateMoniker.Action)
            {
                case UpdateMonikerAction.AddAsDefault:
                case UpdateMonikerAction.Add:
                    Add = new VirtualAddress
                    {
                        Address = bytes,
                        IsDefault = updateParameters.UpdateMoniker.Action == UpdateMonikerAction.AddAsDefault
                    };
                    break;
                case UpdateMonikerAction.Remove:
                    Remove = bytes;
                    break;
            }
        }
    }
}