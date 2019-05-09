using System;

namespace Hashgraph
{
#pragma warning disable CS8618 // Non-nullable field is uninitialized.
    public class TransactionRecord
    {
        public TxId Id { get; internal set; }
        public ResponseCode Status { get; internal set; }
        public ReadOnlyMemory<byte> Hash { get; internal set; }
        public DateTime Concensus { get; internal set; }
        public string Memo { get; internal set; }
        public ulong Fee { get; internal set; }
    }
#pragma warning restore CS8618 // Non-nullable field is uninitialized.
}
