using System;

namespace Hashgraph
{
    /// <summary>
    /// Represents a pre-signed transaction request.  
    /// This structure is passed to each configured 
    /// <see cref="Signatory"/> and signatory callback
    /// method to be given the opportunity to sign the
    /// request before submitting it to the netwwork.
    /// Typically, the signatory will use its private
    /// key to sign the <see cref="TxBytes"/> serialized
    /// representation of the transaciton request.  
    /// This is the same series of bytes that are sent 
    /// to the network along with the signatures 
    /// collected from the signatories.
    /// </summary>
    public interface IInvoice
    {
        /// <summary>
        /// The transaction ID assigned to this request. It,
        /// by its nature, contains a timestamp and expiration.
        /// Any callback methods must return from signing this
        /// transaction with enough time for the transaction to
        /// be submitted to the network with sufficient time to
        /// process before becomming invalid.
        /// </summary>
        public TxId TxId { get; }
        /// <summary>
        /// The memo associated with this transaction, 
        /// for convenience.
        /// </summary>
        public string Memo { get; }
        /// <summary>
        /// The bytes created by serializing the request, including
        /// necessary cryptocurrency transfers, into the underlying
        /// network's protobuf format.  This is the exact sequence
        /// of bytes that will be submitted to the network along side
        /// the signatures created authorizing the request.
        /// </summary>
        public ReadOnlyMemory<byte> TxBytes { get; }
        /// <summary>
        /// Adds a signature to the internal list of signatures
        /// authorizing this request.
        /// </summary>
        /// <param name="type">
        /// The type of signing key used for this signature.
        /// </param>
        /// <param name="publicPrefix">
        /// The first few bytes of the public key associated
        /// with this signature.  This helps the system match
        /// signing requrements held internally in the form of
        /// public keys with the signatures provided.
        /// </param>
        /// <param name="signature">
        /// The bytes representing the signature corresponding
        /// to the associated private/public key.
        /// </param>
        public void AddSignature(KeyType type, ReadOnlyMemory<byte> publicPrefix, ReadOnlyMemory<byte> signature);
    }
}
