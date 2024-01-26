#pragma warning disable CS8618
using System;
using System.Text;
using System.Text.Json;

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
    public ConsensusTimeStamp Concensus { get; internal init; }
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

public record TopicMessage<T> 
{
    private T _content;

    /// <summary>
    /// The content of the message.
    /// </summary>
    public T Content
    {
        get => _content; 
    }
    public string StringContent { get; set; }

    /// <summary>
    /// The Message's Topic.
    /// </summary>
    public Address Topic { get; internal init; }
    /// <summary>
    /// The consensus timestamp.
    /// </summary>
    public ConsensusTimeStamp Concensus { get; internal init; }
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
    
    public void SetContent(T content)
    {
        _content = content;
    }
}