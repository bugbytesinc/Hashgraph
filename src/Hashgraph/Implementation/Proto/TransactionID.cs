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
        internal TxId ToTxId()
        {
            return new TxId(AccountID.ToAddress(), TransactionValidStart.Seconds, TransactionValidStart.Nanos, Scheduled);
        }
    }
}
