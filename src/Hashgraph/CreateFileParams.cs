#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    public class CreateFileParams
    {
        public DateTime Expiration { get; set; }
        public Endorsements Endorsements { get; set; }
        public ReadOnlyMemory<byte> Contents { get; set; }
    }
}
