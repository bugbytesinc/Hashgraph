using Google.Protobuf;
using Grpc.Core;
using Grpc.Net.Client;
using Hashgraph;
using Hashgraph.Implementation;
using System;
using System.Threading;

namespace Proto;

public sealed partial class ConsensusSubmitMessageTransactionBody : INetworkTransaction
{
    SchedulableTransactionBody INetworkTransaction.CreateSchedulableTransactionBody()
    {
        return new SchedulableTransactionBody { ConsensusSubmitMessage = this };
    }

    TransactionBody INetworkTransaction.CreateTransactionBody()
    {
        return new TransactionBody { ConsensusSubmitMessage = this };
    }

    Func<Transaction, Metadata?, DateTime?, CancellationToken, AsyncUnaryCall<TransactionResponse>> INetworkTransaction.InstantiateNetworkRequestMethod(GrpcChannel channel)
    {
        return new ConsensusService.ConsensusServiceClient(channel).submitMessageAsync;
    }

    void INetworkTransaction.CheckReceipt(NetworkResult result)
    {
        if (result.Receipt.Status != ResponseCodeEnum.Success)
        {
            throw new TransactionException(string.Format("Submit Message failed, status: {0}", result.Receipt.Status), result);
        }
    }

    internal ConsensusSubmitMessageTransactionBody(Hashgraph.Address topic, ReadOnlyMemory<byte> message, bool isSegment, Hashgraph.TxId? parentTx, int segmentIndex, int segmentTotalCount) : this()
    {
        if (message.IsEmpty)
        {
            throw new ArgumentOutOfRangeException(nameof(message), "Topic Message can not be empty.");
        }
        TopicID = new TopicID(topic);
        Message = ByteString.CopyFrom(message.Span);
        ChunkInfo = isSegment ? createChunkInfo(parentTx, segmentIndex, segmentTotalCount) : null;

        static ConsensusMessageChunkInfo createChunkInfo(Hashgraph.TxId? parentTx, int segmentIndex, int segmentTotalCount)
        {
            if (segmentTotalCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Hashgraph.SubmitMessageParams.TotalSegmentCount), "Total Segment Count must be a positive number.");
            }
            if (segmentIndex > segmentTotalCount || segmentIndex < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(Hashgraph.SubmitMessageParams.Index), "Segment index must be between one and the total segment count inclusively.");
            }
            if (segmentIndex == 1)
            {
                if (!(parentTx is null))
                {
                    throw new ArgumentOutOfRangeException(nameof(Hashgraph.SubmitMessageParams.ParentTxId), "The Parent Transaction cannot be specified (must be null) when the segment index is one.");
                }
                return new ConsensusMessageChunkInfo
                {
                    Total = segmentTotalCount,
                    Number = segmentIndex,
                    // This is done elsewhere, 
                    // requires smelly edge case workaround
                    //InitialTransactionID = transactionId
                };
            }
            if (parentTx is null)
            {
                throw new ArgumentNullException(nameof(Hashgraph.SubmitMessageParams.ParentTxId), "The parent transaction id is required when segment index is greater than one.");
            }
            return new ConsensusMessageChunkInfo
            {
                Total = segmentTotalCount,
                Number = segmentIndex,
                InitialTransactionID = new TransactionID(parentTx)
            };
        }
    }
}