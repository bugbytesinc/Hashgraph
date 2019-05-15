#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    public sealed class AppendFileParams
    {
        public Address File { get; set; }
        public ReadOnlyMemory<byte> Contents { get; set; }
    }
}
