using Grpc.Core;
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

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
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
        if (Hashgraph.Endorsement.None.Equals(updateParameters.Endorsement))
        {
            throw new ArgumentOutOfRangeException(nameof(updateParameters.Endorsement), "Endorsement can not be 'None', it must contain at least one key requirement.");
        }
        if (updateParameters.Endorsement is null &&
            updateParameters.RequireReceiveSignature is null &&
            updateParameters.Expiration is null &&
            updateParameters.AutoRenewPeriod is null &&
            updateParameters.Proxy is null &&
            updateParameters.Memo is null &&
            updateParameters.AutoAssociationLimit is null &&
            updateParameters.Alias is null)
        {
            throw new ArgumentException(nameof(updateParameters), "The Account Updates contains no update properties, it is blank.");
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
        if (updateParameters.Expiration.HasValue)
        {
            ExpirationTime = new Timestamp(updateParameters.Expiration.Value);
        }
        if (updateParameters.Proxy is not null)
        {
            ProxyAccountID = new AccountID(updateParameters.Proxy);
        }
        if (updateParameters.Memo is not null)
        {
            Memo = updateParameters.Memo;
        }
        if (updateParameters.AutoAssociationLimit is not null)
        {
            var limit = updateParameters.AutoAssociationLimit.Value;
            if (limit < 0 || limit > 1000)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.AutoAssociationLimit), "The maximum number of auto-associaitons must be between zero and 1000.");
            }
            MaxAutomaticTokenAssociations = updateParameters.AutoAssociationLimit.Value;
        }
        if (updateParameters.Alias is not null)
        {
            //Alias = new Key(updateParameters.Alias.Endorsement).ToByteString();
        }
    }
}