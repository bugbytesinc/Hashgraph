using Grpc.Core;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto
{
    public sealed partial class ConsensusDeleteTopicTransactionBody : INetworkTransaction
    {
        SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
        {
            return new SchedulableTransactionBody { ConsensusDeleteTopic = this };
        }

        TransactionBody INetworkTransaction.CreateTransactionBody()
        {
            return new TransactionBody { ConsensusDeleteTopic = this };
        }

        Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(Channel channel)
        {
            return new ConsensusService.ConsensusServiceClient(channel).deleteTopicAsync;
        }

        void INetworkTransaction.CheckReceipt(NetworkResult result)
        {
            if (result.Receipt.Status != ResponseCodeEnum.Success)
            {
                throw new TransactionException(string.Format("Unable to Delete Topic, status: {0}", result.Receipt.Status), result);
            }
        }

        internal ConsensusDeleteTopicTransactionBody(Address topic) : this()
        {
            TopicID = new TopicID(topic);
        }
    }
}
