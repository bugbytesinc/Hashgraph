using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ConsensusUpdateTopicTransactionBody : INetworkTransaction
    {
        string INetworkTransaction.TransactionExceptionMessage => "Unable to update Topic, status: {0}";

        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { ConsensusUpdateTopic = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ConsensusUpdateTopic = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new ConsensusService.ConsensusServiceClient(channel).updateTopicAsync;
        }

        internal ConsensusUpdateTopicTransactionBody(UpdateTopicParams updateParameters) : this()
        {
            TopicID = new TopicID(updateParameters.Topic); if (updateParameters is null)
            {
                throw new ArgumentNullException(nameof(updateParameters), "Topic Update Parameters argument is missing. Please check that it is not null.");
            }
            if (updateParameters.Topic is null)
            {
                throw new ArgumentNullException(nameof(updateParameters.Topic), "Topic address is missing. Please check that it is not null.");
            }
            if (updateParameters.Memo is null &&
                updateParameters.Administrator is null &&
                updateParameters.Participant is null &&
                updateParameters.RenewPeriod is null &&
                updateParameters.RenewAccount is null)
            {
                throw new ArgumentException("The Topic Updates contain no update properties, it is blank.", nameof(updateParameters));
            }
            if (updateParameters.Memo != null)
            {
                Memo = updateParameters.Memo;
            }
            if (!(updateParameters.Administrator is null))
            {
                AdminKey = new Key(updateParameters.Administrator);
            }
            if (!(updateParameters.Participant is null))
            {
                SubmitKey = new Key(updateParameters.Participant);
            }
            if (updateParameters.RenewPeriod.HasValue)
            {
                AutoRenewPeriod = new Duration(updateParameters.RenewPeriod.Value);
            }
            if (!(updateParameters.RenewAccount is null))
            {
                AutoRenewAccount = new AccountID(updateParameters.RenewAccount);
            }
        }
    }
}
