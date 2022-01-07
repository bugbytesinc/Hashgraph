using Hashgraph;

namespace Proto;

public sealed partial class ConsensusMessageChunkInfo
{
    internal Hashgraph.MessageSegmentInfo ToMessageSegmentInfo()
    {
        return new MessageSegmentInfo(this);
    }
}