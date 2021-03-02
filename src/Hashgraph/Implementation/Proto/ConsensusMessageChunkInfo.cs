namespace Proto
{
    public sealed partial class ConsensusMessageChunkInfo
    {
        internal Hashgraph.MessageSegmentInfo ToMessageSegmentInfo()
        {
            return new Hashgraph.MessageSegmentInfo
            {
                ParentTxId = InitialTransactionID?.ToTxId() ?? Hashgraph.TxId.None,
                Index = Number,
                TotalSegmentCount = Total
            };
        }
    }
}