using System;

namespace Hashgraph
{
    public sealed class PrecheckException : Exception
    {
        public ResponseCode Status { get; private set; }
        public TxId TxId { get; private set; }
        public PrecheckException(string message, TxId transaction,  ResponseCode code) : base(message)
        {
            Status = code;
            TxId = transaction;
        }
    }
}
