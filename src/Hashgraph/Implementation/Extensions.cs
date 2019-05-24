using Google.Protobuf;
using System;

namespace Hashgraph.Implementation
{
    /// <summary>
    /// Internal Helper Convenience Extensions
    /// </summary>
    internal static class Extensions
    {
        internal static ByteString ToByteString(this ReadOnlyMemory<byte> data)
        {
            return ByteString.CopyFrom(data.ToArray());
        }
    }
}
