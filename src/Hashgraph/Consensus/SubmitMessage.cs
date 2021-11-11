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
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, null, configure, false).ConfigureAwait(false));
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
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, signatory, configure, false).ConfigureAwait(false));
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
            return new SubmitMessageReceipt(await SubmitMessageImplementationAsync(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure, false).ConfigureAwait(false));
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
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, null, configure, true).ConfigureAwait(false));
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
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(topic, message, false, null, 0, 0, signatory, configure, true).ConfigureAwait(false));
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
            return new SubmitMessageRecord(await SubmitMessageImplementationAsync(submitParams.Topic, submitParams.Segment, true, submitParams.ParentTxId, submitParams.Index, submitParams.TotalSegmentCount, submitParams.Signatory, configure, true).ConfigureAwait(false));
        }
        /// <summary>
        /// Internal implementation of the submit message call.
        /// </summary>
        private async Task<NetworkResult> SubmitMessageImplementationAsync(Address topic, ReadOnlyMemory<byte> message, bool isSegment, TxId? parentTx, int segmentIndex, int segmentTotalCount, Signatory? signatory, Action<IContext>? configure, bool includeRecord)
        {
            var transaction = new ConsensusSubmitMessageTransactionBody(topic, message, isSegment, parentTx, segmentIndex, segmentTotalCount);
            if (isSegment && segmentIndex == 1)
            {
                // Smelly Workaround due to necesity to embed the
                // same transaction ID in the middle of the message
                // as the envelope for the case of the first segment
                // of a segmented message.
                //
                // First We need to apply the confgure command, to 
                // create the correct context
                await using var configuredClient = Clone(configure);
                var initialChunkTransactionId = configuredClient._context.GetOrCreateTransactionID();
                if (configuredClient._context.GatherSignatories(signatory).GetSchedule() is null)
                {
                    // The easy path, this is not a scheduled transaction.
                    transaction.ChunkInfo!.InitialTransactionID = initialChunkTransactionId;
                }
                else
                {
                    // Even more smell, we need to check to see if this is a
                    // scheduled transaction.  If this is, the initial chunk 
                    // transaction should have the "scheduled" flag set.
                    var scheduledChunkTransactionId = new TransactionID(initialChunkTransactionId);
                    scheduledChunkTransactionId.Scheduled = true;
                    transaction.ChunkInfo!.InitialTransactionID = scheduledChunkTransactionId;
                }
                // We use our configured client, however we need to override the
                // configuration with one additional configuration rule that will
                // peg the transaction to our pre-computed value.
                var result = await configuredClient.ExecuteTransactionAsync(transaction, ctx => ctx.Transaction = initialChunkTransactionId.AsTxId(), false, signatory).ConfigureAwait(false);
                if (includeRecord)
                {
                    // Note: we use the original context here, because we 
                    // don't want to re-use the transaction ID that was pinned
                    // to the subContext, would cause the paying TX to fail as a duplicate.
                    result.Record = (await configuredClient.ExecuteQueryInContextAsync(new TransactionGetRecordQuery(result.TransactionID, false), configuredClient._context, 0).ConfigureAwait(false)).TransactionGetRecord.TransactionRecord;
                }
                return result;
            }
            else
            {
                return await ExecuteTransactionAsync(transaction, configure, includeRecord, signatory).ConfigureAwait(false);
            }
        }
    }
}
