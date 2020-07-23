#pragma warning disable CS8618
using System;
using System.Collections.Generic;
using System.Text;

namespace Hashgraph
{
    /// <summary>
    /// Optional metadata that may be attached to an
    /// Segmented HCS message identifying the index
    /// of the segment and which parent message this
    /// segment correlates with.
    /// </summary>
    public class MessageSegmentInfo
    {
        /// <summary>
        /// The transaction the created the first segment
        /// of the message.  This acts as a correlation
        /// identifier to coalesce the segments of the
        /// message int one.
        /// </summary>
        public TxId ParentTxId { get; internal set; }
        /// <summary>
        /// The index of this segment (one based).
        /// </summary>
        public int Index { get; internal set; }
        /// <summary>
        /// The total number of segments making up
        /// the whole of the message when assembled.
        /// </summary>
        public int TotalSegmentCount { get; internal set; }
    }
}
