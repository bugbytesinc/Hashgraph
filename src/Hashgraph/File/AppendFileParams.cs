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
    }
}
