#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Detailed description of a network file.
    /// </summary>
    public sealed class FileInfo
    {
        /// <summary>
        /// The network address of the file.
        /// </summary>
        public Address File { get; internal set; }
        /// <summary>
        /// The size of the file in bytes (plus 30 extra for overhead).
        /// </summary>
        public long Size { get; internal set; }
        /// <summary>
        /// The file expiration date at which it will be removed from 
        /// the network.  The date can be extended thru updates.
        /// </summary>
        public DateTime Expiration { get; internal set; }
        /// <summary>
        /// A descriptor of the keys required to sign transactions editing and 
        /// otherwise manipulating the contents of this file.
        /// </summary>
        public Endorsements Endorsements { get; internal set; }
        /// <summary>
        /// Flag indicating the file has been deleted.
        /// </summary>
        public bool Deleted { get; internal set; }
    }
}
