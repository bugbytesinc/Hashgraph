#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// File content append parameters.
    /// </summary>
    public sealed class AppendFileParams
    {
        /// <summary>
        /// The file receiving the appended content.
        /// </summary>
        public Address File { get; set; }
        /// <summary>
        /// The content to append to the file, in bytes.
        /// </summary>
        public ReadOnlyMemory<byte> Contents { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to append to this file.  Typically matches the
        /// Endorsement associated with this file.
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
