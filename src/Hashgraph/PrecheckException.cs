using System;

namespace Hashgraph
{
    public sealed class PrecheckException : Exception
    {
        public PrecheckResponse PrecheckResponseCode { get; private set; }
        public Transaction Transaction { get; private set; }
        public PrecheckException(string message, Transaction transaction,  PrecheckResponse code) : base(message)
        {
            PrecheckResponseCode = code;
            Transaction = transaction;
        }
    }
}
