﻿using Grpc.Core;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class CryptoCreateTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to create account, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { CryptoCreateAccount = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { CryptoCreateAccount = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new CryptoService.CryptoServiceClient(channel).createAccountAsync;
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
            Key = new Key(createParameters.Endorsement);
            InitialBalance = createParameters.InitialBalance;
            ReceiverSigRequired = createParameters.RequireReceiveSignature;
            AutoRenewPeriod = new Duration(createParameters.AutoRenewPeriod);
            ProxyAccountID = createParameters.Proxy is null ? null : new AccountID(createParameters.Proxy);
            Memo = createParameters.Memo ?? string.Empty;
        }
    }
}
