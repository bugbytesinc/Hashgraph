#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// The information returned from the GetTopicInfo Client 
    /// method call.  It represents the details concerning a 
    /// Hedera Network Consensus Topic.
    /// </summary>
    public sealed record TopicInfo
    {
        /// <summary>
        /// The memo associated with the topic instance.
        /// </summary>
        public string Memo { get; internal init; }
        /// <summary>
        /// A SHA-384 Running Hash of the following: Previous RunningHash,
        /// TopicId, ConsensusTimestamp, SequenceNumber and Message
        /// </summary>
        public ReadOnlyMemory<byte> RunningHash { get; internal init; }
        /// <summary>
        /// The number of Messages submitted to this topic at the
        /// time of the call to Get Topic Info.
        /// </summary>
        public ulong SequenceNumber { get; internal init; }
        /// <summary>
        /// The Time after which this topic will no longer accept
        /// messages.  The topic will automatically be deleted after
        /// the system defined grace period beyond the expiration time.
        /// </summary>
        public DateTime Expiration { get; internal init; }
        /// <summary>
        /// An endorsement, when specified, can be used to 
        /// authorize a modification or deletion of this topic, including
        /// control of topic lifetime extensions. Additionally, if null, 
        /// any account can extend the topic lifetime.
        /// </summary>
        public Endorsement? Administrator { get; internal init; }
        /// <summary>
        /// Identifies the key requirements for submitting messages to this topic.
        /// If blank, any account may submit messages to this topic, 
        /// otherwise they must meet the specified signing requirements.
        /// </summary>
        public Endorsement? Participant { get; internal init; }
        /// <summary>
        /// Incremental period for auto-renewal of the topic. If
        /// auto-renew account does not have sufficient funds to renew 
        /// at the expiration time, it will be renewed for a period of 
        /// time the remaining funds can support.  If no funds remain, 
        /// the topic will be deleted.
        /// </summary>
        public TimeSpan AutoRenewPeriod { get; internal init; }
        /// <summary>
        /// Address of the account supporting the auto renewal of 
        /// the topic at expiration time.  The topic lifetime will be
        /// extended by the RenewPeriod at expiration time if this account
        /// contains sufficient funds.
        public Address? RenewAccount { get; internal init; }
    }
}
