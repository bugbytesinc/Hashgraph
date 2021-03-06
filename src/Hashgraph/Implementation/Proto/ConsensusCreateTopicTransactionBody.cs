﻿using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ConsensusCreateTopicTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to create Consensus Topic, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { ConsensusCreateTopic = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ConsensusCreateTopic = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new ConsensusService.ConsensusServiceClient(channel).createTopicAsync;
        }

        internal ConsensusCreateTopicTransactionBody(CreateTopicParams createParameters) : this()
        {
            if (createParameters is null)
            {
                throw new ArgumentNullException(nameof(createParameters), "The create parameters are missing. Please check that the argument is not null.");
            }
            if (createParameters.Memo is null)
            {
                throw new ArgumentNullException(nameof(createParameters.Memo), "Memo can not be null.");
            }
            if (!(createParameters.RenewAccount is null) && createParameters.Administrator is null)
            {
                throw new ArgumentNullException(nameof(createParameters.Administrator), "The Administrator endorssement must not be null if RenewAccount is specified.");
            }
            if (createParameters.RenewPeriod.Ticks < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(createParameters.RenewPeriod), "The renew period must be greater than zero, and typically less than or equal to 90 days.");
            }
            Memo = createParameters.Memo;
            AdminKey = createParameters.Administrator is null ? null : new Key(createParameters.Administrator);
            SubmitKey = createParameters.Participant is null ? null : new Key(createParameters.Participant);
            AutoRenewPeriod = new Duration(createParameters.RenewPeriod);
            AutoRenewAccount = createParameters.RenewAccount is null ? null : new AccountID(createParameters.RenewAccount);
        }
    }
}
