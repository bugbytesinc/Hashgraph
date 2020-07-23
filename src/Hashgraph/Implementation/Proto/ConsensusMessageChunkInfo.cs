namespace Proto
{
    public sealed partial class ConsensusMessageChunkInfo
    {
        internal Hashgraph.MessageSegmentInfo ToMessageSegmentInfo()
        {
            return new Hashgraph.MessageSegmentInfo
            {
                ParentTxId = InitialTransactionID?.ToTxId() ?? new Hashgraph.TxId(),
                Index = Number,
                TotalSegmentCount = Total
            };
        }
    }
}