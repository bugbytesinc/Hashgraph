using Google.Protobuf;
using Hashgraph.Implementation;
using Proto;
using System;
using System.Threading.Tasks;

namespace Hashgraph
{
    public partial class Client
    {
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, null, configure, false));
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the key signing requirements for participants.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageReceipt> SubmitMessageAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, signatory, configure, false));
        }
        /// <summary>
        /// Sends a segment of a message to the network for a given consensus topic.
        /// The caller of this method is responsible for managing the segment of the
        /// message and associated metadata.
        /// </summary>
        /// <param name="submitParams">
        /// Details of the message segment to upload, including the metadata
        /// corresponding to this segment.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageReceipt> SubmitMessageAsync(SubmitMessageParams submitParams, Action<IContext>? configure = null)
        {
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure, false));
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Receipt indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Action<IContext>? configure = null)
        {
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, null, configure, true));
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="topic">
        /// The address of the topic for the message.
        /// </param>
        /// <param name="message">
        /// The value of the message, limited to the 4K total Network Transaction Size.
        /// </param>
        /// <param name="signatory">
        /// The signatory containing any additional private keys or callbacks
        /// to meet the key signing requirements for participants.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Record indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(Address topic, ReadOnlyMemory<byte> message, Signatory signatory, Action<IContext>? configure = null)
        {
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, signatory, configure, true));
        }
        /// <summary>
        /// Sends a message to the network for a given consensus topic.
        /// </summary>
        /// <param name="submitParams">
        /// Details of the message segment to upload, including the metadata
        /// corresponding to this segment.
        /// </param>
        /// <param name="configure">
        /// Optional callback method providing an opportunity to modify 
        /// the execution configuration for just this method call. 
        /// It is executed prior to submitting the request to the network.
        /// </param>
        /// <returns>
        /// A Submit Message Record indicating success, includes information
        /// about the sequence number of the message and its running hash.
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
        /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
        /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
        /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
        /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>
        public async Task<SubmitMessageRecord> SubmitMessageWithRecordAsync(SubmitMessageParams submitParams, Action<IContext>? configure = null)
        {
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure, true));
        }
        /// <summary>
        /// Internal implementation of the submit message call.
        /// </summary>
        private async Task<NetworkResult> SubmitMessageImplementationAsync(Address topic, ReadOnlyMemory<byte> message, bool isSegment, TxId? parentTx, int segmentIndex, int segmentTotalCount, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            topic = RequireInputParameter.Topic(topic);
            message = RequireInputParameter.Message(message);
            await using var context = CreateChildContext(configure);
            var transactionBody = new TransactionBody
            {
                ConsensusSubmitMessage = new ConsensusSubmitMessageTransactionBody
                {
                    TopicID = new TopicID(topic),
                    Message = ByteString.CopyFrom(message.Span),
                    ChunkInfo = isSegment ? createChunkInfo(parentTx, segmentIndex, segmentTotalCount) : null
                }
            };
            if (isSegment && segmentIndex == 1)
            {
                // Smelly Workaround due to necesity to embed the
                // same transaction ID in the middle of the message
                // as the envelope for the case of the first segment
                // of a segmented message.
                var initialChunkTransactionId = context.GetOrCreateTransactionID();
                await using var subContext = new GossipContextStack(context);
                subContext.Transaction = initialChunkTransactionId.AsTxId();
                if (subContext.GatherSignatories(signatory).GetSchedule() is null)
                {
                    transactionBody.ConsensusSubmitMessage.ChunkInfo!.InitialTransactionID = initialChunkTransactionId;
                }
                else
                {
                    // Even more smell, we need to check to see if this is a
                    // scheduled transaction.  If this is, the initial chunk 
                    // transaction should have the "scheduled" flag set.
                    var scheduledChunkTransactionId = new TransactionID(initialChunkTransactionId);
                    scheduledChunkTransactionId.Scheduled = true;
                    transactionBody.ConsensusSubmitMessage.ChunkInfo!.InitialTransactionID = scheduledChunkTransactionId;
                }
                var result = await transactionBody.SignAndExecuteWithRetryAsync(subContext, false, "Submit Message failed, status: {0}", signatory);
                if (includeRecord)
                {
                    // Note: we use the original context here, because we 
                    // don't want to re-use the transaction ID that was pinned
                    // to the subContext, would cause the paying TX to fail as a duplicate.
                    result.Record = await context.GetTransactionRecordAsync(result.TransactionID);
                }
                return result;
            }
            else
            {
                return await transactionBody.SignAndExecuteWithRetryAsync(context, includeRecord, "Submit Message failed, status: {0}", signatory);
            }

            static ConsensusMessageChunkInfo createChunkInfo(TxId? parentTx, int segmentIndex, int segmentTotalCount)
            {
                if (segmentTotalCount < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.TotalSegmentCount), "Total Segment Count must be a positive number.");
                }
                if (segmentIndex > segmentTotalCount || segmentIndex < 1)
                {
                    throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.Index), "Segment index must be between one and the total segment count inclusively.");
                }
                if (segmentIndex == 1)
                {
                    if (!(parentTx is null))
                    {
                        throw new ArgumentOutOfRangeException(nameof(SubmitMessageParams.ParentTxId), "The Parent Transaction cannot be specified (must be null) when the segment index is one.");
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
                    throw new ArgumentNullException(nameof(SubmitMessageParams.ParentTxId), "The parent transaction id is required when segment index is greater than one.");
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
}
