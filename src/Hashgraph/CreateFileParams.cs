#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    /// <summary>
    /// File creation parameters.
    /// </summary>
    public class CreateFileParams
    {
        /// <summary>
        /// Original expiration date for the file, fees will be charged as appropriate.
        /// </summary>
        public DateTime Expiration { get; set; }
        /// <summary>
        /// A descriptor of the keys required to sign transactions editing and 
        /// otherwise manipulating the contents of this file.
        /// </summary>
        public Endorsements Endorsements { get; set; }
        /// <summary>
        /// The initial contents of the file.
        /// </summary>
        public ReadOnlyMemory<byte> Contents { get; set; }
    }
}
