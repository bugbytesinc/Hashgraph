﻿using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class TokenUpdateTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { TokenUpdate = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { TokenUpdate = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new TokenService.TokenServiceClient(channel).updateTokenAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Unable to update Token, status: {0}", result.Receipt.Status), result);
        }
    }

    internal TokenUpdateTransactionBody(Hashgraph.UpdateTokenParams updateParameters) : this()
    {
        if (updateParameters is null)
        {
            throw new ArgumentNullException(nameof(updateParameters), "Token Update Parameters argument is missing. Please check that it is not null.");
        }
        if (updateParameters.Token.IsNullOrNone())
        {
            throw new ArgumentNullException(nameof(updateParameters.Token), "The Token is missing.  Please check that it is not null or empty.");
        }
        if (updateParameters.Treasury is null &&
            updateParameters.Administrator is null &&
            updateParameters.GrantKycEndorsement is null &&
            updateParameters.SuspendEndorsement is null &&
            updateParameters.PauseEndorsement is null &&
            updateParameters.ConfiscateEndorsement is null &&
            updateParameters.SupplyEndorsement is null &&
            updateParameters.RoyaltiesEndorsement is null &&
            string.IsNullOrWhiteSpace(updateParameters.Symbol) &&
            string.IsNullOrWhiteSpace(updateParameters.Name) &&
            !updateParameters.Expiration.HasValue &&
            !updateParameters.RenewPeriod.HasValue &&
            updateParameters.RenewAccount is null &&
            updateParameters.Memo is null)
        {
            throw new ArgumentException("The Topic Updates contain no update properties, it is blank.", nameof(updateParameters));
        }
        Token = new TokenID(updateParameters.Token);
        if (!string.IsNullOrWhiteSpace(updateParameters.Symbol))
        {
            if (updateParameters.Symbol.Trim().Length != updateParameters.Symbol.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot contain leading or trailing white space.");
            }
            if (updateParameters.Symbol.Length > 100)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Symbol), "The new token symbol cannot exceed 100 characters in length.");
            }
            Symbol = updateParameters.Symbol;
        }
        if (!string.IsNullOrWhiteSpace(updateParameters.Name))
        {
            if (updateParameters.Name.Trim().Length != updateParameters.Name.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Name), "The new token name cannot contain leading or trailing white space.");
            }
            Name = updateParameters.Name;
        }
        if (updateParameters.Expiration.HasValue)
        {
            if (updateParameters.Expiration.Value < ConsensusTimeStamp.Now)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Expiration), "The new expiration can not be set to the past.");
            }
            Expiry = new Timestamp(updateParameters.Expiration.Value);
        }
        if (updateParameters.RenewPeriod.HasValue)
        {
            if (updateParameters.RenewPeriod.Value.TotalSeconds < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.RenewPeriod), "The renew period must be non negative.");
            }
            AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
        }
        if (updateParameters.Memo is not null)
        {
            if (updateParameters.Memo.Trim().Length != updateParameters.Memo.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(updateParameters.Memo), "The new token memo cannot contain leading or trailing white space.");
            }
            Memo = updateParameters.Memo;
        }
        if (updateParameters.Treasury is not null)
        {
            Treasury = new AccountID(updateParameters.Treasury);
        }
        if (updateParameters.Administrator is not null)
        {
            AdminKey = new Key(updateParameters.Administrator);
        }
        if (updateParameters.GrantKycEndorsement is not null)
        {
            KycKey = new Key(updateParameters.GrantKycEndorsement);
        }
        if (updateParameters.SuspendEndorsement is not null)
        {
            FreezeKey = new Key(updateParameters.SuspendEndorsement);
        }
        if (updateParameters.PauseEndorsement is not null)
        {
            PauseKey = new Key(updateParameters.PauseEndorsement);
        }
        if (updateParameters.ConfiscateEndorsement is not null)
        {
            WipeKey = new Key(updateParameters.ConfiscateEndorsement);
        }
        if (updateParameters.SupplyEndorsement is not null)
        {
            SupplyKey = new Key(updateParameters.SupplyEndorsement);
        }
        if (updateParameters.RoyaltiesEndorsement is not null)
        {
            FeeScheduleKey = new Key(updateParameters.RoyaltiesEndorsement);
        }
        if (updateParameters.RenewAccount is not null)
        {
            AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
        }
    }
}