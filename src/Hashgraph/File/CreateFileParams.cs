#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// File creation parameters.
    /// </summary>
    public sealed class CreateFileParams
    {
        /// <summary>
        /// Original expiration date for the file, fees will be charged as appropriate.
        /// </summary>
        public DateTime Expiration { get; set; }
        /// <summary>
        /// A descriptor of the keys required to sign transactions editing and 
        /// otherwise manipulating the contents of this file.  Only one key
        /// is required to sign the transaction to delete the file.
        /// </summary>
        public Endorsement[] Endorsements { get; set; }
        /// <summary>
        /// The initial contents of the file.
        /// </summary>
        public ReadOnlyMemory<byte> Contents { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to create to this file.  Typically matches the
        /// Endorsements associated with this file.
        /// </summary>
        /// <remarks>
        /// Keys/callbacks added here will be combined with those already
        /// identified in the client object's context when signing this 
        /// transaction to change the state of this account.  They will 
        /// not be asked to sign transactions to retrieve the record
        /// if the "WithRecord" form of the method call is made.  The
        /// client will rely on the Signatory from the context to sign
        /// the transaction requesting the record.
        /// </remarks>
        public Signatory? Signatory { get; set; }
    }
}
