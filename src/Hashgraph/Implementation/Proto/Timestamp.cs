using Hashgraph;

namespace Proto;

public sealed partial class Timestamp
{
    internal Timestamp(ConsensusTimeStamp consensusTimeStamp) : this()
    {
        var seconds = decimal.Truncate(consensusTimeStamp.Seconds);
        Seconds = (long)seconds;
        Nanos = (int)decimal.Multiply(decimal.Subtract(consensusTimeStamp.Seconds, seconds), 1000000000m);
    }
    internal ConsensusTimeStamp ToConsensusTimeStamp()
    {
        return new ConsensusTimeStamp(Seconds, Nanos);
    }
}