using Proto;
using System;

namespace Hashgraph
{
    /// <summary>
    /// Detailed description of a network file.
    /// </summary>
    public sealed record FileInfo
    {
        /// <summary>
        /// The network address of the file.
        /// </summary>
        public Address File { get; private init; }
        /// <summary>
        /// A short description of the file.
        /// </summary>
        public string Memo { get; private init; }
        /// <summary>
        /// The size of the file in bytes (plus 30 extra for overhead).
        /// </summary>
        public long Size { get; private init; }
        /// <summary>
        /// The file expiration date at which it will be removed from 
        /// the network.  The date can be extended thru updates.
        /// </summary>
        public DateTime Expiration { get; private init; }
        /// <summary>
        /// A descriptor of the all the keys required to sign transactions 
        /// editing and otherwise manipulating the contents of this file.
        /// </summary>
        public Endorsement[] Endorsements { get; private init; }
        /// <summary>
        /// Flag indicating the file has been deleted.
        /// </summary>
        public bool Deleted { get; private init; }
        /// <summary>
        /// Intenral Constructor from Raw Response
        /// </summary>
        internal FileInfo(Response response)
        {
            var info = response.FileGetInfo.FileInfo;
            File = info.FileID.AsAddress();
            Memo = info.Memo;
            Size = info.Size;
            Expiration = info.ExpirationTime.ToDateTime();
            Endorsements = info.Keys?.ToEndorsements() ?? Array.Empty<Endorsement>();
            Deleted = info.Deleted;
        }
    }
}
