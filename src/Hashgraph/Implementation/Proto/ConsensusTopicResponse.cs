namespace Com.Hedera.Mirror.Api.Proto;

public sealed partial class ConsensusTopicResponse
{
    public Hashgraph.TopicMessage ToTopicMessage(Hashgraph.Address topic)
    {
        return new Hashgraph.TopicMessage
        {
            Topic = topic,
            Concensus = ConsensusTimestamp.ToConsensusTimeStamp(),
            Messsage = Message.Memory,
            RunningHash = RunningHash.Memory,
            SequenceNumber = SequenceNumber,
            SegmentInfo = ChunkInfo?.ToMessageSegmentInfo()
        };
    }
    
    public Hashgraph.TopicMessage<T> ToTopicMessage<T>(Hashgraph.Address topic)
    {
        return new Hashgraph.TopicMessage<T>
        {
            Topic = topic,
            Concensus = ConsensusTimestamp.ToConsensusTimeStamp(),
            Messsage = Message.Memory,
            RunningHash = RunningHash.Memory,
            SequenceNumber = SequenceNumber,
            SegmentInfo = ChunkInfo?.ToMessageSegmentInfo()
        };
    }
}