#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// Parameters to set the period of time where the network will suspened will stop 
    /// creating events and accepting transactions. This can be used to safely shut down 
    /// the platform for maintenance and for upgrades if the file information is included.
    /// </summary>
    public sealed class SuspendNetworkParams
    {
        /// <summary>
        /// The amount time to wait before the the network should deactivate.
        /// </summary>
        public TimeSpan Starting { get; set; }
        /// <summary>
        /// The period of time the network should remain deactivated.
        /// </summary>
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Optional Address of the File to be loaded if this suspension is
        /// part of a maintanance update. (Typically the network will only
        /// accept a special well known file number.)
        /// </summary>
        public Address UpdateFile { get; set; }
        /// <summary>
        /// Hash value of the contents of the upgrade file (if included).
        /// The network relies on this hash to verify the file contents
        /// before performing the maintanance update.
        /// </summary>
        public ReadOnlyMemory<byte> UpdateFileHash { get; set; }
    }
}
