using System;

namespace Hashgraph
{
    public sealed class TransactionException : Exception
    {
        public TransactionRecord TransactionRecord { get; private set; }
        public TransactionException(string message, TransactionRecord transactionRecord) : base(message)
        {
            TransactionRecord = transactionRecord;
        }
        public TransactionException(string message, TransactionRecord transactionRecord, Exception innerException) : base(message, innerException)
        {
            TransactionRecord = transactionRecord;
        }
    }
}
