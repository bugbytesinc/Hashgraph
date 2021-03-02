#pragma warning disable CS8618
using System;

namespace Hashgraph
{
    /// <summary>
    /// Pending Transaction Sign Parameters
    /// </summary>
    public sealed class SignPendingTransactionParams
    {
        /// <summary>
        /// The identifier of the pending transaction 
        /// record held by the network.
        /// </summary>
        public Address Pending { get; set; }
        /// <summary>
        /// The body of the pending transaciton, serialized
        /// into the binary protobuf message format.
        /// </summary>
        public ReadOnlyMemory<byte> TransactionBody { get; set; }
        /// <summary>
        /// The Signatory(ies) that will sign the pending transaction
        /// bytes identified by the <code>TransactionBody</code> property.
        /// If none are specified here, the method will use the signatories
        /// held in the context instead for signing.  
        /// </summary>
        /// <remarks>
        /// NOTE: This is unlike other transactions where all signatures
        /// appear signing the outer envelope of the transaction 
        /// (both the context and "extra" signatories").  This is an 
        /// either/or relationship, if nothing is specified here, the
        /// signatores in the conext will be called twice.  Once to 
        /// sign the <code>TransactionBody</code> payload, and then
        /// again to sign the "envelope" transaction as a whole.
        /// </remarks>
        public Signatory? Signatory { get; set; }
    }
}
