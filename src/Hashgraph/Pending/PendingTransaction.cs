#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// The identifer of a pending transaction, includes the address
    /// ID identifying the transaction within the network and the 
    /// body bytes of the transaction that must be signed by the
    /// appropriate signatories to invoke execution.
    /// </summary>
    public record PendingTransaction
    {
        /// <summary>
        /// The identifier of the pending transaction 
        /// record held by the network.
        /// </summary>
        public Address Pending { get; internal init; }
        /// <summary>
        /// The body of the pending transaciton, serialized
        /// into the binary protobuf message format.
        /// </summary>
        public ReadOnlyMemory<byte> TransactionBody { get; internal init; }
    }
}
