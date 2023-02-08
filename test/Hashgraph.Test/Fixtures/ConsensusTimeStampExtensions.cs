using System;

namespace Hashgraph.Test.Fixtures;

public static class ConsensusTimeStampExtensions
{
    public static ConsensusTimeStamp AddMinutes(this ConsensusTimeStamp timeStamp, int minutes)
    {
        return new ConsensusTimeStamp(DateTime.UnixEpoch.AddSeconds((double)timeStamp.Seconds).AddMinutes(minutes));
    }
}