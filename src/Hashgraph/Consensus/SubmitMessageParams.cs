#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// Advanced Submit Message Parameters,
    /// includes Segment Information. 
    /// </summary>
    /// <remarks>
    /// The hedera network does not validate
    /// the segment information submitted to 
    /// a consensus topic.  This metadata must
    /// be validated upon consumption and there
    /// can be gaps and inconsistencies in the
    /// resulting mirror HCS stream for the
    /// related topic.
    /// </remarks>
    public sealed class SubmitMessageParams
    {
        /// <summary>
        /// The address of the topic for the message.
        /// </summary>
        public Address Topic { get; set; }
        /// <summary>
        /// The value of this segment of the message,
        /// limited to the 4K total Network Transaction Size.
        /// </summary>
        public ReadOnlyMemory<byte> Segment { get; set; }
        /// <summary>
        /// The transaction the created the first segment
        /// of the message.  This acts as a correlation
        /// identifier to coalesce the segments of the
        /// message int one.
        /// </summary>
        /// <remarks>
        /// This must be left to be null when sending
        /// the first segment of a message.  The 
        /// value of the transaction ID returned from
        /// the receipt or record will contain the value
        /// assocated with this parameter for the first
        /// segment.  This value must be included in 
        /// subsequent segments for this message.
        /// </remarks>
        public TxId ParentTxId { get; set; }
        /// <summary>
        /// The index of this segment (one based).
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// The total number of segments making up
        /// the whole of the message when assembled.
        /// </summary>
        public int TotalSegmentCount { get; set; }
        /// <summary>
        /// The signatory containing any additional 
        /// private keys or callbacks to meet the key 
        /// signing requirements for participants.
        /// </summary>
        public Signatory? Signatory { get; set; }
    }
}
