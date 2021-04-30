#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Input parameters describing how to update a network file.
    /// </summary>
    public sealed class UpdateFileParams
    {
        /// <summary>
        /// The address of the network file to update.
        /// </summary>
        public Address File { get; set; }
        /// <summary>
        /// If not null, a new description of the file.
        /// </summary>
        public string? Memo { get; set; }
        /// <summary>
        /// A descriptor of the keys required to sign transactions editing and 
        /// otherwise manipulating the contents of this file. Set to
        /// <code>null</code> to leave unchanged.
        /// </summary>
        public Endorsement[]? Endorsements { get; set; }
        /// <summary>
        /// Replace the contents of the file with these new contents.  Set to
        /// <code>null</code> to leave the existing content unchanged.
        /// </summary>
        public ReadOnlyMemory<byte>? Contents { get; set; }
        /// <summary>
        /// Additional private key, keys or signing callback method 
        /// required to replace the contents of this file.  Typically
        /// matchs all the Endorsements in the Endorsement array
        /// associated with this file.
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
