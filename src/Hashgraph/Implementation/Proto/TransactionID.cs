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
        }
        internal TxId ToTxId()
        {
            return new TxId
            {
                Address = AccountID.ToAddress(),
                ValidStartSeconds = TransactionValidStart.Seconds,
                ValidStartNanos = TransactionValidStart.Nanos
            };
        }
    }
}
