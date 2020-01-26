#pragma warning disable CS8618 // Non-nullable field is uninitialized.

using System;

namespace Hashgraph
{
    /// <summary>
    /// Record produced from creating a new contract.
    /// </summary>
    public sealed class SubmitMessageRecord : TransactionRecord
    {
        /// <summary>
        /// A SHA-384 Running Hash of the following: Previous RunningHash,
        /// TopicId, ConsensusTimestamp, SequenceNumber and this Message
        /// Submission.
        /// </summary>
        public ReadOnlyMemory<byte> RunningHash { get; internal set; }
        /// <summary>
        /// The sequence number of this message submission.
        /// </summary>
        public ulong SequenceNumber { get; internal set; }
    }
}