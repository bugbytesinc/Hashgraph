using Hashgraph;

namespace Proto
{
    public sealed partial class TransactionID
    {
        internal TransactionID(TxId transaction) : this()
        {
            AccountID = new AccountID(transaction.Address);
            TransactionValidStart = new Timestamp
            {
                Seconds = transaction.ValidStartSeconds,
                Nanos = transaction.ValidStartNanos
            };
            Scheduled = transaction.Pending;
        }
    }

    internal static class TransactionIDExtensions
    {
        internal static TxId AsTxId(this TransactionID? id)
        {
            if(id is not null)
            {
                var timestamp = id.TransactionValidStart;
                if(timestamp is not null)
                {
                    return new TxId(id.AccountID.AsAddress(), timestamp.Seconds, timestamp.Nanos, id.Scheduled);
                }
            }
            return TxId.None;
        }
    }
}
