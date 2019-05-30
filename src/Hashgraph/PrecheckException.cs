using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents the condition where a submitted transaction 
    /// failed the pre-check validation by the network gateway node.
    /// </summary>
    public sealed class PrecheckException : Exception
    {
        /// <summary>
        /// The status code returned by the gateway node.
        /// </summary>
        public ResponseCode Status { get; private set; }
        /// <summary>
        /// The Transaction ID corresponding to the request that failed.
        /// </summary>
        public TxId TxId { get; private set; }
        /// <summary>
        /// Exception constructor.
        /// </summary>
        /// <param name="message">
        /// A message describing the nature of the problem.
        /// </param>
        /// <param name="transaction">
        /// The Transaction ID corresponding to the request that failed.
        /// </param>
        /// <param name="code">
        /// The status code returned by the gateway node.
        /// </param>
        public PrecheckException(string message, TxId transaction, ResponseCode code) : base(message)
        {
            Status = code;
            TxId = transaction;
        }
        /// <summary>
        /// Exception constructor.
        /// </summary>
        /// <param name="message">
        /// A message describing the nature of the problem.
        /// </param>
        /// <param name="transaction">
        /// The Transaction ID corresponding to the request that failed.
        /// </param>
        /// <param name="innerException">
        /// Inner exception causing this error, typically reserved for
        /// fundamental GRPC pipeline exceptions.
        /// </param>
        /// <param name="code">
        /// The status code returned by the gateway node.
        /// </param>
        public PrecheckException(string message, TxId transaction, ResponseCode code, Exception innerException) : base(message, innerException)
        {
            Status = code;
            TxId = transaction;
        }
    }
}
