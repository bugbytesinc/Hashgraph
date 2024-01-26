#pragma warning disable CS8618
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace Hashgraph;

public class SubscribeTopicInput<T>
{
    /// <summary>
    /// The Message's Topic.
    /// </summary>
    public Address Topic { get; set; }
    /// <summary>
    /// Optional, filter for messages which reached consensus on or 
    /// after this time. If not set, messages occurring from the
    /// current time forward are returned.
    /// </summary>
    public ConsensusTimeStamp? Starting { get; set; } = null;
    /// <summary>
    /// Optional, filter for messages which reached consensus before 
    /// this time. If not set, it will stream indefinitely.
    /// </summary>
    public ConsensusTimeStamp? Ending { get; set; } = null;
    /// <summary>
    /// Optional, the maximum number of topic messages to return before
    /// completing the call, if set to 0 it will stream messages 
    /// indefinitely until the stream terminates by other means.
    /// </summary>
    public ulong MaxCount { get; set; } = 0;
    
    /// <summary>
    /// Method to be called when a message is received from the mirror node.
    /// </summary>
    public Func<TopicMessage<T>,Task> SubscribeMethod { get; set; }
}