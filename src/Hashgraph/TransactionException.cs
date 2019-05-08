using System;

namespace Hashgraph
{
    public sealed class TransactionException : Exception
    {
        public TransactionException(string message) : base(message)
        {
        }
        public TransactionException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
