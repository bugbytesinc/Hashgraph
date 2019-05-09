using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents an error with a transaction that passed the gateway node 
    /// pre-check and was processed by the network.  It contains the transaction 
    /// record providing details about the transaction and insight into why 
    /// it may have failed.
    /// </summary>
    public sealed class TransactionException : Exception
    {
        /// <summary>
        /// The transaction record associated with the request and resonse.
        /// </summary>
        public TransactionRecord TransactionRecord { get; private set; }
        /// <summary>
        /// Exception constructor.
        /// </summary>
        /// <param name="message">
        /// Message describing the nature of the error.
        /// </param>
        /// <param name="transactionRecord">
        /// The transaction record object associated with the request.  
        /// May contain information useful for troubleshooting the error.
        /// </param>
        public TransactionException(string message, TransactionRecord transactionRecord) : base(message)
        {
            TransactionRecord = transactionRecord;
        }
    }
}
