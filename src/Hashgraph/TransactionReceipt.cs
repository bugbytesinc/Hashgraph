#pragma warning disable CS8618 // Non-nullable field is uninitialized.

namespace Hashgraph
{
    /// <summary>
    /// The details returned from the network after consensus 
    /// has been reached for a network request.
    /// </summary>
    public class TransactionReceipt
    {
        /// <summary>
        /// The Transaction ID associated with the request.
        /// </summary>
        public TxId Id { get; internal set; }
        /// <summary>
        /// The response code returned from the server.
        /// </summary>
        public ResponseCode Status { get; internal set; }
    }
}
