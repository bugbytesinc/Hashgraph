#pragma warning disable CS8618 // Non-nullable field is uninitialized.
using System;

namespace Hashgraph
{
    public sealed class FileInfo
    {
        public Address File { get; internal set; }
        public long Size { get; internal set; }
        public DateTime Expiration { get; internal set; }
        public Endorsements Endorsements { get; internal set; }
        public bool Deleted { get; internal set; }
    }
}
