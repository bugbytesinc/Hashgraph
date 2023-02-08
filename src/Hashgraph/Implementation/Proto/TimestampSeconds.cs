using Hashgraph;

namespace Proto;

public sealed partial class TimestampSeconds
{
    internal ConsensusTimeStamp ToConsensusTimeStamp()
    {
        return new ConsensusTimeStamp(Seconds);
    }
}