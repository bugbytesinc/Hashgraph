using Proto;

namespace Com.Hedera.Mirror.Api.Proto
{
    public sealed partial class ConsensusTopicResponse
    {
        internal Hashgraph.TopicMessage ToTopicMessage(Hashgraph.Address topic)
        {
            return new Hashgraph.TopicMessage
            {
                Topic = topic,
                Concensus = ConsensusTimestamp.ToDateTime(),
                Messsage = Message.Memory,
                RunningHash = RunningHash.Memory,
                SequenceNumber = SequenceNumber,
                SegmentInfo = ChunkInfo?.ToMessageSegmentInfo()
            };
        }
    }
}