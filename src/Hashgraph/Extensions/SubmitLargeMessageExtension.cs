#pragma warning disable CS8604 // Possible null reference argument.
using System;
using System.Threading.Tasks;

namespace Hashgraph.Extensions;

/// <summary>
/// Extends the client functionality to include the 
/// orcestration of sending a large segmented
/// consensus message.
/// </summary>
public static class SubmitLargeMessageExtension
{
    /// <summary>
    /// Sends an HCS message of arbitrary size
    /// to the network by breaking the message
    /// into segments, submitting each segment
    /// in sequence.  Manages the segment metadata
    /// internally, returning an array of receipts
    /// representing the transactions required to
    /// upload the entier message.
    /// </summary>
    /// <param name="client">
    /// A Hashgraph Client instance.
    /// </param>
    /// <param name="topic">
    /// The address of the topic for the message.
    /// </param>
    /// <param name="message">
    /// The value of the message, may exceed the
    /// network limit size.
    /// </param>
    /// <param name="segmentSize">
    /// The maximum size of each segment.  Must
    /// be under the current allowed size for 
    /// transactions currently supported by the 
    /// network.  The method will break the 
    /// message into as many segments as necessary
    /// to fulfill uploading the entire message.
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
    /// An array of Submit Message Receipts indicating success, one for each
    /// segment uploaded.  The Transaction ID of the first receipt matches
    /// the correlation transaction ID for the series of message segments
    /// as a whole.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">If required arguments are missing.</exception>
    /// <exception cref="InvalidOperationException">If required context configuration is missing.</exception>
    /// <exception cref="PrecheckException">If the gateway node create rejected the request upon submission.</exception>
    /// <exception cref="ConsensusException">If the network was unable to come to consensus before the duration of the transaction expired.</exception>
    /// <exception cref="TransactionException">If the network rejected the create request as invalid or had missing data.</exception>

    public static async Task<SubmitMessageReceipt[]> SubmitLargeMessageAsync(this Client client, Address topic, ReadOnlyMemory<byte> message, int segmentSize, Signatory? signatory = null, Action<IContext>? configure = null)
    {
        await using var configuredClient = client.Clone(configure);
        var segmentCount = (message.Length - 1) / segmentSize + 1;
        var receipts = new SubmitMessageReceipt[segmentCount];
        receipts[0] = await configuredClient.SubmitMessageAsync(new SubmitMessageParams
        {
            Topic = topic,
            Segment = segmentCount > 1 ? message.Slice(0, segmentSize) : message,
            Index = 1,
            TotalSegmentCount = segmentCount,
            Signatory = signatory
        }).ConfigureAwait(false);
        var parentTx = receipts[0].Id;
        for (int i = 1; i < segmentCount - 1; i++)
        {
            receipts[i] = await configuredClient.SubmitMessageAsync(new SubmitMessageParams
            {
                Topic = topic,
                Segment = message.Slice(segmentSize * i, segmentSize),
                ParentTxId = parentTx,
                Index = i + 1,
                TotalSegmentCount = segmentCount,
                Signatory = signatory
            }).ConfigureAwait(false);
        }
        if (segmentCount > 1)
        {
            receipts[segmentCount - 1] = await configuredClient.SubmitMessageAsync(new SubmitMessageParams
            {
                Topic = topic,
                Segment = message.Slice(segmentSize * (segmentCount - 1)),
                ParentTxId = parentTx,
                Index = segmentCount,
                TotalSegmentCount = segmentCount,
                Signatory = signatory
            }).ConfigureAwait(false);
        }
        return receipts;
    }
}