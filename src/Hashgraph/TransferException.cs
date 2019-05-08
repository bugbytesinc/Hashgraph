using System;

namespace Hashgraph
{
    public sealed class TransferException : Exception
    {
        public TransferException(string message) : base(message)
        {
        }
        public TransferException(string message, Exception innerException) : base(message, innerException)
        {
        }
    }
}
