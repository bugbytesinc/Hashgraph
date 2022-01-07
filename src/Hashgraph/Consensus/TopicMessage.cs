#pragma warning disable CS8618
using System;

namespace Hashgraph;

/// <summary>
/// Represents a Topic Message retrieved from a mirror node.
/// </summary>
public sealed record TopicMessage
{
    /// <summary>
    /// The Message's Topic.
    /// </summary>
    public Address Topic { get; internal init; }
    /// <summary>
    /// The consensus timestamp.
    /// </summary>
    public DateTime Concensus { get; internal init; }
    /// <summary>
    /// The content of the message.
    /// </summary>
    public ReadOnlyMemory<byte> Messsage { get; internal init; }
    /// <summary>
    /// A SHA-384 Running Hash of the following: Previous RunningHash,
    /// TopicId, ConsensusTimestamp, SequenceNumber and this Message
    /// Submission.
    /// </summary>
    public ReadOnlyMemory<byte> RunningHash { get; internal init; }
    /// <summary>
    /// The sequence number of this message submission.
    /// </summary>
    public ulong SequenceNumber { get; internal init; }
    /// <summary>
    /// Optional metadata that may be attached to an
    /// Segmented HCS message identifying the index
    /// of the segment and which parent message this
    /// segment correlates with.
    /// </summary>
    public MessageSegmentInfo? SegmentInfo { get; internal init; }
}