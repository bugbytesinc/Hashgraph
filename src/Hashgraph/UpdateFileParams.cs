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
        /// A descriptor of the keys required to sign transactions editing and 
        /// otherwise manipulating the contents of this file. Set to
        /// <code>null</code> to leave unchanged.
        /// </summary>
        public Endorsements? Endorsements { get; set; }
        /// <summary>
        /// Replace the contents of the file with these new contents.  Set to
        /// <code>null</code> to leave the existing content unchanged.
        /// </summary>
        public ReadOnlyMemory<byte>? Contents { get; set; }
    }
}
